using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using JetBrains.Annotations;

namespace CardioMonitor.Ui.Storyboards
{
    public class StoryboardsNavigationServce : IDisposable
    {
        private IStroryboardPageCreator _pageCreator;
        private readonly Dictionary<InnerStoryboardPageInfo, PageCreationInfo> _registeredPages;
        private readonly Dictionary<Guid, StoryboardPageView> _cachedPages;
        private readonly LinkedList<InnerStoryboardPageInfo> _journal;
        private readonly Dictionary<Guid, Storyboard> _storyboards;
        private readonly Dictionary<Guid, IPageContext> _pageContexts;
        private bool _isStartPagesCreated;

        
        private Storyboard _activeStoryboard;

        private InnerStoryboardPageInfo _activeInnerStoryboardPageInfo;

        public event EventHandler<Guid> ActiveStoryboardChanged; 

        public StoryboardsNavigationServce()
        {
            _registeredPages = new Dictionary<InnerStoryboardPageInfo, PageCreationInfo>();
            _cachedPages = new Dictionary<Guid, StoryboardPageView>();
            _journal = new LinkedList<InnerStoryboardPageInfo>();
            _storyboards = new Dictionary<Guid, Storyboard>();
            _pageContexts = new Dictionary<Guid, IPageContext>();
            _isStartPagesCreated = false;
        }

        public void SetStoryboardPageCreator([NotNull] IStroryboardPageCreator pageCreator)
        {
            _pageCreator = pageCreator ?? throw new ArgumentNullException(nameof(pageCreator));
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

        public void CreateStartPages()
        {
            if (_isStartPagesCreated) throw new InvalidOperationException("Start pages have been alread created");

            foreach (var storyboard in _storyboards)
            {
                var startPageInfo =
                    _registeredPages.Keys.FirstOrDefault(x => x.IsStartPage && x.StoryboardId == storyboard.Key);
                if (startPageInfo == null) throw new InvalidOperationException($"Start page for storyboard {storyboard.Key} not registered");
                
                var view = CreatePageView(startPageInfo);
                storyboard.Value.ActivePage = view;
            }

            _isStartPagesCreated = true;
        }

        [NotNull]
        private StoryboardPageView CreatePageView([NotNull] InnerStoryboardPageInfo pageInfo)
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

        private void ViewModelOnPageTransitionRequested(object sender, [NotNull] TransitionRequest transitionRequest)
        {
            if (transitionRequest == null) throw new ArgumentNullException(nameof(transitionRequest));

            var viewModel = sender as IStoryboardViewModel;
            if (viewModel == null) throw new InvalidOperationException("Incorrect request of transition");


            GoToPage(transitionRequest.DestinationPageId, viewModel.StoryboardId, transitionRequest.DestinationPageContext);
        }

        private void ViewModelOnPageCompleted(object sender, EventArgs eventArgs)
        {
            HandleViewModelTransitions(sender, PageTransitionTrigger.Completed);
        }

        private void HandleViewModelTransitions(object sender, PageTransitionTrigger trigger)
        {
            var viewModel = sender as IStoryboardViewModel;
            if (viewModel == null) throw new InvalidOperationException("Incorrect request of transition");

            var storyboard = _storyboards.Values.FirstOrDefault(x => x.StoryboardId == viewModel.StoryboardId);
            if (storyboard == null) throw new InvalidOperationException("Storyboard not found");

            var destinationPageId = storyboard.GetDestinationPage(viewModel.PageId, trigger);
            GoToPage(destinationPageId, storyboard.StoryboardId);

        }

        private void ViewModelOnPageCanceled(object sender, EventArgs eventArgs)
        {
            HandleViewModelTransitions(sender, PageTransitionTrigger.Canceled);
        }

        private void ViewModelOnPageBackRequested(object sender, EventArgs eventArgs)
        {
            HandleViewModelTransitions(sender, PageTransitionTrigger.Back);
        }

        public void GoToPage(Guid pageId, Guid? storyboardId = null, IPageContext pageContext = null)
        {
            var storyBoard = storyboardId.HasValue
                ? _storyboards.FirstOrDefault(x => x.Key == storyboardId.Value).Value
                : _activeStoryboard;

            var pageInfo = storyBoard != null
                ? _registeredPages.Keys.FirstOrDefault(x =>
                    x.PageId == pageId && x.StoryboardId == storyBoard.StoryboardId)
                : _registeredPages.Keys.FirstOrDefault(x => x.PageId == pageId);
            if (pageInfo == null) throw new InvalidOperationException("Page not found");

            OpenPage(pageInfo, pageContext);
        }

        private void OpenPage([NotNull] InnerStoryboardPageInfo pageInfo, IPageContext pageContext = null)
        {
            if (!_storyboards.ContainsKey(pageInfo.StoryboardId))
            {
                throw new InvalidOperationException($"Storyboard {pageInfo.StoryboardId} does not registered");
            }
            var storyboard = _storyboards[pageInfo.StoryboardId];

            if (_activeStoryboard?.ActivePage != null)
            {
                var previousPage = _activeStoryboard.ActivePage;
                var viewModel = previousPage.ViewModel;
                if (!viewModel.CanLeave()) return;

                viewModel.Leave();
            }

            //todo support types
            if (_cachedPages.ContainsKey(pageInfo.PageUniqueId))
            {
                var page = _cachedPages[pageInfo.PageUniqueId];
               
                storyboard.ActivePage = page;
                _pageContexts.TryGetValue(pageInfo.PageUniqueId, out var restoredPageContext);
                page.ViewModel.Return(restoredPageContext);
            }
            else
            {
                var view = CreatePageView(pageInfo);
                storyboard.ActivePage = view;
                view.ViewModel.Open(pageContext);
                _pageContexts[pageInfo.PageUniqueId] = pageContext;
            }
            if (_activeInnerStoryboardPageInfo == null || _activeInnerStoryboardPageInfo.StoryboardId != pageInfo.StoryboardId)
            {
                ActiveStoryboardChanged?.Invoke(this, pageInfo.StoryboardId);
            }

            _journal.AddLast(pageInfo);
            _activeInnerStoryboardPageInfo = pageInfo;
            _activeStoryboard = storyboard;
        }

       
        public void GoToStoryboard(Guid storyboardId)
        {
            var pageInfo = _journal.FirstOrDefault(x => x.StoryboardId == storyboardId);
            if (pageInfo == null)
            {
                pageInfo =
                    _registeredPages.Keys.FirstOrDefault(x => x.StoryboardId == storyboardId && x.IsStartPage);
                if (pageInfo == null) throw new InvalidOperationException("Pages for storyboard does not registered");

            }
            OpenPage(pageInfo);
        }
        

        public void GoBack()
        {
            if (!CanGoBack()) return;
            if (_activeInnerStoryboardPageInfo == null) return;

            var lastPageFromStoryboard = _journal.LastOrDefault(x =>
                x.StoryboardId == _activeInnerStoryboardPageInfo.StoryboardId
                && x.PageId != _activeInnerStoryboardPageInfo.PageId);

            if (lastPageFromStoryboard == null)
            {
                lastPageFromStoryboard =
                    _journal.LastOrDefault(x => x.StoryboardId != _activeInnerStoryboardPageInfo.StoryboardId);
                if (lastPageFromStoryboard == null) return;
            }

           
            _journal.Remove(_activeInnerStoryboardPageInfo);

            //start pages always in memory
            if (_cachedPages.ContainsKey(_activeInnerStoryboardPageInfo.PageUniqueId) && !_activeInnerStoryboardPageInfo.IsStartPage)
            {
                _cachedPages.Remove(_activeInnerStoryboardPageInfo.PageUniqueId);

                if (_activeStoryboard?.ActivePage != null)
                {
                    var previousPage = _activeStoryboard.ActivePage;
                    var viewModel = previousPage.ViewModel;
                    if (!viewModel.CanClose()) return;

                    viewModel.PageBackRequested -= ViewModelOnPageBackRequested;
                    viewModel.PageCanceled -= ViewModelOnPageCanceled;
                    viewModel.PageCompleted -= ViewModelOnPageCompleted;
                    viewModel.PageTransitionRequested -= ViewModelOnPageTransitionRequested;

                    viewModel.Close();
                }
            }

            OpenPage(lastPageFromStoryboard);
        }

        public bool CanGoBack()
        {
            if (_activeStoryboard?.ActivePage == null) return false;
            if (_storyboards.Count < 0) return false;
            // if no page or one there is no reason to execute back command
            if (_journal.Count < 2) return false;
            // if opened only start pages there is no reason to execute back command
            if (_journal.All(x => x.IsStartPage)) return false;

            if (_activeStoryboard?.ActivePage != null)
            {
                var previousPage = _activeStoryboard.ActivePage;
                var viewModel = previousPage.ViewModel;
                if (!viewModel.CanClose()) return false;
            }


            //todo add support of changing CanGoBack
            return true;
        }
        

        public void Dispose()
        {
            //todo
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


    public interface IStroryboardPageCreator
    {
        StoryboardPageView CreateView(Type type);

        IStoryboardViewModel CreateViewModel(Type type);
    }

    public class StoryboardPageView : UIElement
    {
        private IStoryboardViewModel _viewModel;

        public IStoryboardViewModel ViewModel
        {
            get => _viewModel;
            set => _viewModel = value;
        }
    }

    public interface IPageContext
    {

    }

}