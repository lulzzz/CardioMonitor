﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public class StoryboardsNavigationService : IDisposable
    {
        private readonly Dictionary<InnerStoryboardPageInfo, PageCreationInfo> _registeredPages;
        private readonly Dictionary<Guid, IStoryboardPageView> _cachedPages;
        private readonly LinkedList<InnerStoryboardPageInfo> _journal;
        private readonly Dictionary<Guid, Storyboard> _storyboards;
        private readonly Dictionary<Guid, IStoryboardPageContext> _pageContexts;
        private readonly Dictionary<Guid, IStoryboardPageContext> _startPageContexts;
        private bool _isStartPagesCreated;

        private readonly Dictionary<Guid, bool> _startPagesOpenningStat;
        
        private Storyboard _activeStoryboard;
        private InnerStoryboardPageInfo _activeInnerStoryboardPageInfo;
        private IStoryboardPageCreator _pageCreator;

        public event EventHandler<Guid> ActiveStoryboardChanged;

        public event EventHandler CanBackChanged;

        private IUiInvoker _invoker;

        public StoryboardsNavigationService()
        {
            _registeredPages = new Dictionary<InnerStoryboardPageInfo, PageCreationInfo>();
            _cachedPages = new Dictionary<Guid, IStoryboardPageView>();
            _journal = new LinkedList<InnerStoryboardPageInfo>();
            _storyboards = new Dictionary<Guid, Storyboard>();
            _pageContexts = new Dictionary<Guid, IStoryboardPageContext>();
            _startPageContexts = new Dictionary<Guid, IStoryboardPageContext>();
            _isStartPagesCreated = false;
            _pageCreator = new DefaultStoryboardCreator();
            _startPagesOpenningStat = new Dictionary<Guid, bool>();
        }

        public void SetStoryboardPageCreator([NotNull] IStoryboardPageCreator pageCreator)
        {
            _pageCreator = pageCreator ?? throw new ArgumentNullException(nameof(pageCreator));
        }
        
        public void SetUiInvoker(IUiInvoker invoker)
        {
            _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        }

        public void RegisterStoryboard([NotNull] Storyboard storyboard)
        {
            if (storyboard == null) throw new ArgumentNullException(nameof(storyboard));
            if (_storyboards.ContainsKey(storyboard.StoryboardId)) throw new ArgumentException("Storyboard with same id already habe been registered");

            _storyboards[storyboard.StoryboardId] = storyboard;

            foreach (var storyboardPageInfo in storyboard.GetPageInfos())
            {
                RegisterPage(storyboard.StoryboardId, storyboardPageInfo);
            }
        }

        private void RegisterPage(Guid storyboardId, [NotNull] StoryboardPageInfo pageInfo)
        {
            if (pageInfo == null) throw new ArgumentNullException(nameof(pageInfo));
            if (pageInfo.View == null) throw new ArgumentException($"{nameof(pageInfo.View)} is null");
            if (pageInfo.ViewModel == null) throw new ArgumentException($"{nameof(pageInfo.ViewModel)} is null");
            
           var pageInstanceInfo = new InnerStoryboardPageInfo
           {
               IsStartPage = pageInfo.IsStartPage,
               PageId = pageInfo.PageId,
               StoryboardId = storyboardId,
               PageUniqueId = Guid.NewGuid()
           };

            var creationInfo = new PageCreationInfo()
            {
                ViewModel = pageInfo.ViewModel,
                View = pageInfo.View
            };

            _registeredPages[pageInstanceInfo] = creationInfo;
        }

        public void CreateStartPages(Dictionary<Guid, IStoryboardPageContext> startPageContexts = null)
        {
            if (_isStartPagesCreated) throw new InvalidOperationException("Start pages have been alread created");

            _invoker.Invoke(() =>
            {
                foreach (var storyboard in _storyboards)
                {
                    var startPageInfo =
                        _registeredPages.Keys.FirstOrDefault(x => x.IsStartPage && x.StoryboardId == storyboard.Key);
                    if (startPageInfo == null)
                        throw new InvalidOperationException(
                            $"Start page for storyboard {storyboard.Key} not registered");

                    var view = CreatePageView(startPageInfo);
                    storyboard.Value.ActivePage = view;
                    _startPagesOpenningStat[startPageInfo.PageUniqueId] = false;
                    IStoryboardPageContext context = null;
                    startPageContexts?.TryGetValue(startPageInfo.PageId, out context);
                    _startPageContexts[startPageInfo.PageUniqueId] = context;
                    _pageContexts[startPageInfo.PageUniqueId] = context;
                }

                _isStartPagesCreated = true;
            });
        }

        [NotNull]
        private IStoryboardPageView CreatePageView([NotNull] InnerStoryboardPageInfo pageInfo)
        {
            if (!_registeredPages.ContainsKey(pageInfo))
            {
                throw new InvalidOperationException($"Page creation info with pageTypeId {pageInfo.PageId} not registered");
            }

            var pageCreationInfo = _registeredPages[pageInfo];
            var view = _pageCreator.CreateView(pageCreationInfo.View);
            var viewModel = _pageCreator.CreateViewModel(pageCreationInfo.ViewModel);
            viewModel.PageId = pageInfo.PageId;
            viewModel.StoryboardId = pageInfo.StoryboardId;
            viewModel.PageBackRequested += ViewModelOnPageBackRequested;
            viewModel.PageCanceled += ViewModelOnPageCanceled;
            viewModel.PageCompleted += ViewModelOnPageCompleted;
            viewModel.PageTransitionRequested += ViewModelOnPageTransitionRequested;
            view.ViewModel = viewModel;
            _cachedPages.Add(pageInfo.PageUniqueId, view);
            return view;
        }

        private Task ViewModelOnPageTransitionRequested(object sender, [NotNull] TransitionRequest transitionRequest)
        {
            if (transitionRequest == null) throw new ArgumentNullException(nameof(transitionRequest));

            return _invoker.InvokeAsync(() =>
            {
                if (!(sender is IStoryboardPageViewModel viewModel)) throw new InvalidOperationException("Incorrect request of transition");

                return GoToPageAsync(transitionRequest.DestinationPageId, viewModel.StoryboardId, transitionRequest.DestinationPageContext);
            });
        }

        private async Task ViewModelOnPageCompleted([NotNull] TrasitionEvent trasitionEvent)
        {
            if (trasitionEvent == null) throw new ArgumentNullException(nameof(trasitionEvent));
            if (!(trasitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");
            
            await HandleViewModelTransitionsAsync(trasitionEvent.Sender, PageTransitionTrigger.Completed, trasitionEvent.Context).ConfigureAwait(false);
        }

        private Task HandleViewModelTransitionsAsync(object sender, PageTransitionTrigger trigger, IStoryboardPageContext context = null)
        {
            return _invoker.InvokeAsync(async () =>
            {
                if (!(sender is IStoryboardPageViewModel viewModel))
                    throw new InvalidOperationException("Incorrect request of transition");

                await RemoveOpennedPageAsync().ConfigureAwait(true);

                var storyboard = _storyboards.Values.FirstOrDefault(x => x.StoryboardId == viewModel.StoryboardId);
                if (storyboard == null) throw new InvalidOperationException("Storyboard not found");

                var destinationPageId = storyboard.GetDestinationPage(viewModel.PageId, trigger);
                await GoToPageAsync(destinationPageId, storyboard.StoryboardId, context).ConfigureAwait(true);
            });
        }

        private Task ViewModelOnPageCanceled([NotNull] TrasitionEvent trasitionEvent)
        {
            if (trasitionEvent == null) throw new ArgumentNullException(nameof(trasitionEvent));
            if (!(trasitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");
            return HandleViewModelTransitionsAsync(trasitionEvent.Sender, PageTransitionTrigger.Canceled, trasitionEvent.Context);
        }

        private Task ViewModelOnPageBackRequested([NotNull] TrasitionEvent trasitionEvent)
        {
            if (trasitionEvent == null) throw new ArgumentNullException(nameof(trasitionEvent));
            if (!(trasitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");

            return HandleViewModelTransitionsAsync(trasitionEvent.Sender, PageTransitionTrigger.Back, trasitionEvent.Context);
        }

        public Task GoToPageAsync(Guid pageId, Guid? storyboardId = null, IStoryboardPageContext pageContext = null)
        {
            return _invoker.InvokeAsync(() =>
            {
                var storyBoard = storyboardId.HasValue
                    ? _storyboards.FirstOrDefault(x => x.Key == storyboardId.Value).Value
                    : _activeStoryboard;

                //first try to find desired page in current storyboard, then look up at all registered pages
                var pageInfo = storyBoard != null
                    ? _registeredPages.Keys.FirstOrDefault(x =>
                        x.PageId == pageId && x.StoryboardId == storyBoard.StoryboardId)
                    : _registeredPages.Keys.FirstOrDefault(x => x.PageId == pageId);
                if (pageInfo == null)
                {
                    pageInfo = _registeredPages.Keys.FirstOrDefault(x => x.PageId == pageId);
                    if (pageInfo == null)
                    {
                        throw new InvalidOperationException("Page not found");
                    }
                }

                return OpenPageAsync(pageInfo, pageContext);
            });
        }

        private Task OpenPageAsync(
            [NotNull] InnerStoryboardPageInfo pageInfo,
            [CanBeNull] IStoryboardPageContext pageContext = null, 
            bool addToJournal = true)
        {
            if (!_storyboards.ContainsKey(pageInfo.StoryboardId))
            {
                throw new InvalidOperationException($"Storyboard {pageInfo.StoryboardId} does not registered");
            }

            return _invoker.InvokeAsync(async () =>
            {
                var storyboard = _storyboards[pageInfo.StoryboardId];

                if (_activeStoryboard?.ActivePage != null)
                {
                    var previousPage = _activeStoryboard.ActivePage;
                    var viewModel = previousPage.ViewModel;

                    var canLeave = await viewModel.CanLeaveAsync().ConfigureAwait(true);
                    if (!canLeave) return;

                    await viewModel.LeaveAsync().ConfigureAwait(true);
                }
                
                _activeInnerStoryboardPageInfo = pageInfo;
                _activeStoryboard = storyboard;
                    ActiveStoryboardChanged?.Invoke(this, pageInfo.StoryboardId);

                if (_cachedPages.ContainsKey(pageInfo.PageUniqueId))
                {
                    var page = _cachedPages[pageInfo.PageUniqueId];

                    storyboard.ActivePage = page;
                    _pageContexts.TryGetValue(pageInfo.PageUniqueId, out var restoredPageContext);

                    _startPagesOpenningStat.TryGetValue(pageInfo.PageUniqueId, out var wasPageOpenned);
                    if (wasPageOpenned)
                    {
                        await page.ViewModel.ReturnAsync(pageContext ?? restoredPageContext).ConfigureAwait(true);
                    }
                    else
                    {
                        await page.ViewModel.OpenAsync(pageContext ?? restoredPageContext).ConfigureAwait(true);
                        _startPagesOpenningStat[pageInfo.PageUniqueId] = true;
                    }

                }
                else
                {
                    var view = CreatePageView(pageInfo);
                    storyboard.ActivePage = view;

                    await view.ViewModel.OpenAsync(pageContext).ConfigureAwait(true);
                    _pageContexts[pageInfo.PageUniqueId] = pageContext;
                }


                if (addToJournal)
                {
                    _journal.AddLast(pageInfo);
                }

                RiseCanBackChanged();
            });
        }

        private void RiseCanBackChanged()
        {
            CanBackChanged?.Invoke(this, EventArgs.Empty);
        }
       
        public async Task GoToStoryboardAsync(Guid storyboardId)
        {
            var pageInfo = _journal.LastOrDefault(x => x.StoryboardId == storyboardId);
            if (pageInfo == null)
            {
                pageInfo =
                    _registeredPages.Keys.FirstOrDefault(x => x.StoryboardId == storyboardId && x.IsStartPage);
                if (pageInfo == null) throw new InvalidOperationException("Pages for storyboard does not registered");

            }
            await OpenPageAsync(pageInfo).ConfigureAwait(true);
        }
        

        public Task GoBackAsync()
        {
            if (!CanGoBack()) return Task.CompletedTask;
            if (_activeInnerStoryboardPageInfo == null) return Task.CompletedTask;

            return _invoker.InvokeAsync(async () =>
            {

                var lastPageFromStoryboard = await RemoveOpennedPageAsync().ConfigureAwait(true);
                if (lastPageFromStoryboard == null) return;
                
                // already have been added
                await OpenPageAsync(lastPageFromStoryboard, addToJournal: false).ConfigureAwait(true);
            });
        }

        private async Task<InnerStoryboardPageInfo> RemoveOpennedPageAsync()
        {
            if (_activeInnerStoryboardPageInfo == null) return null;

            // start pages does not close
            if (_activeStoryboard?.ActivePage != null && 
                !_activeInnerStoryboardPageInfo.IsStartPage)
            {
                var previousPage = _activeStoryboard.ActivePage;
                var viewModel = previousPage.ViewModel;
                var canClose = await viewModel.CanCloseAsync().ConfigureAwait(true);
                if (!canClose) return null;
            }

            var lastPageFromStoryboard =  _activeInnerStoryboardPageInfo.IsStartPage
                ? _journal.LastOrDefault(x =>
                    x.StoryboardId != _activeInnerStoryboardPageInfo.StoryboardId)
                : _journal.LastOrDefault(x =>
                    x.StoryboardId == _activeInnerStoryboardPageInfo.StoryboardId
                    && x.PageId != _activeInnerStoryboardPageInfo.PageId);

            if (lastPageFromStoryboard == null)
            {
                lastPageFromStoryboard =
                    _journal.LastOrDefault(x => x.StoryboardId != _activeInnerStoryboardPageInfo.StoryboardId);
                if (lastPageFromStoryboard == null) return null;
            }


            _journal.Remove(_activeInnerStoryboardPageInfo);

            //start pages always in memory
            if (_cachedPages.ContainsKey(_activeInnerStoryboardPageInfo.PageUniqueId) &&
                !_activeInnerStoryboardPageInfo.IsStartPage)
            {
                _cachedPages.Remove(_activeInnerStoryboardPageInfo.PageUniqueId);

                if (_activeStoryboard?.ActivePage != null)
                {
                    var previousPage = _activeStoryboard.ActivePage;
                    var viewModel = previousPage.ViewModel;
                    var canClose = await viewModel.CanCloseAsync().ConfigureAwait(true);
                    if (!canClose) return null;

                    viewModel.PageBackRequested -= ViewModelOnPageBackRequested;
                    viewModel.PageCanceled -= ViewModelOnPageCanceled;
                    viewModel.PageCompleted -= ViewModelOnPageCompleted;
                    viewModel.PageTransitionRequested -= ViewModelOnPageTransitionRequested;

                    await viewModel.CloseAsync().ConfigureAwait(true);
                }
            }

            if (_pageContexts.ContainsKey(_activeInnerStoryboardPageInfo.PageUniqueId))
            {
                _pageContexts.Remove(_activeInnerStoryboardPageInfo.PageUniqueId);
            }

            //todo maybe add isShared flag?
            if (_activeInnerStoryboardPageInfo.IsStartPage &&
                _startPageContexts.ContainsKey(_activeInnerStoryboardPageInfo.PageUniqueId))
            {
                _pageContexts[_activeInnerStoryboardPageInfo.PageUniqueId] =
                    _startPageContexts[_activeInnerStoryboardPageInfo.PageUniqueId];
            }

            return lastPageFromStoryboard;
        }

        public bool CanGoBack()
        {
            return _invoker.Invoke(() => {
                if (_activeStoryboard?.ActivePage == null) return false;
                if (_storyboards.Count < 0) return false;
                // if no page or one there is no reason to execute back command
                if (_journal.Count < 2) return false;
                // if opened only start pages there is no reason to execute back command
                if (_journal.All(x => x.IsStartPage)) return false;

                return true;
            });
        }


        public void Dispose()
        {
            _invoker.Invoke(() =>
            {
                foreach (var page in _cachedPages)
                {
                    var viewModel = page.Value?.ViewModel;
                    viewModel?.Dispose();
                }
            });
        }

        //todo add can close app support

        private class PageCreationInfo
        {
            public Type View { get; set; }

            public Type ViewModel { get; set; }
        }

        private class InnerStoryboardPageInfo
        {
            /// <summary>
            /// Unique Id for page within all storyboards
            /// </summary>
            public Guid PageUniqueId { get; set; }

            public Guid PageId { get; set; }

            public Guid StoryboardId { get; set; }

            public bool IsStartPage { get; set; }
        }
    }
}