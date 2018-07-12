using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    //todo add can close app support
    public class StoryboardsNavigationService : IDisposable
    {
        protected readonly Dictionary<InnerStoryboardPageInfo, PageCreationInfo> RegisteredPages;
        protected readonly Dictionary<Guid, IStoryboardPageView> CachedPages;
        protected readonly LinkedList<InnerStoryboardPageInfo> Journal;
        protected readonly Dictionary<Guid, Storyboard> Storyboards;
        protected readonly Dictionary<Guid, IStoryboardPageContext> PageContexts;
        protected readonly Dictionary<Guid, IStoryboardPageContext> StartPageContexts;
        protected bool IsStartPagesCreated;

        protected readonly Dictionary<Guid, bool> StartPagesOpenningStat;

        protected Storyboard ActiveStoryboard;
        protected InnerStoryboardPageInfo ActiveInnerStoryboardPageInfo;
        protected IStoryboardPageCreator PageCreator;

        public event EventHandler<Guid> ActiveStoryboardChanged;

        public event EventHandler CanBackChanged;

        protected IUiInvoker Invoker;

        public event EventHandler<Exception> ExceptionOccured;

        public StoryboardsNavigationService()
        {
            RegisteredPages = new Dictionary<InnerStoryboardPageInfo, PageCreationInfo>();
            CachedPages = new Dictionary<Guid, IStoryboardPageView>();
            Journal = new LinkedList<InnerStoryboardPageInfo>();
            Storyboards = new Dictionary<Guid, Storyboard>();
            PageContexts = new Dictionary<Guid, IStoryboardPageContext>();
            StartPageContexts = new Dictionary<Guid, IStoryboardPageContext>();
            IsStartPagesCreated = false;
            PageCreator = new DefaultStoryboardCreator();
            StartPagesOpenningStat = new Dictionary<Guid, bool>();
        }

        public void SetStoryboardPageCreator([NotNull] IStoryboardPageCreator pageCreator)
        {
            PageCreator = pageCreator ?? throw new ArgumentNullException(nameof(pageCreator));
        }
        
        public void SetUiInvoker(IUiInvoker invoker)
        {
            Invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        }

        public void RegisterStoryboard([NotNull] Storyboard storyboard)
        {
            if (storyboard == null) throw new ArgumentNullException(nameof(storyboard));
            if (Storyboards.ContainsKey(storyboard.StoryboardId)) throw new ArgumentException("Storyboard with same id already habe been registered");

            Storyboards[storyboard.StoryboardId] = storyboard;

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

            var creationInfo = new PageCreationInfo
            {
                ViewModel = pageInfo.ViewModel,
                View = pageInfo.View
            };

            RegisteredPages[pageInstanceInfo] = creationInfo;
        }

        public void CreateStartPages(Dictionary<Guid, IStoryboardPageContext> startPageContexts = null)
        {
            if (IsStartPagesCreated) throw new InvalidOperationException("Start pages have been alread created");

            Invoker.Invoke(() =>
            {
                foreach (var storyboard in Storyboards)
                {
                    var startPageInfo =
                        RegisteredPages.Keys.FirstOrDefault(x => x.IsStartPage && x.StoryboardId == storyboard.Key);
                    if (startPageInfo == null)
                        throw new InvalidOperationException(
                            $"Start page for storyboard {storyboard.Key} not registered");

                    var view = CreatePageView(startPageInfo);
                    storyboard.Value.ActivePage = view;
                    StartPagesOpenningStat[startPageInfo.PageUniqueId] = false;
                    IStoryboardPageContext context = null;
                    startPageContexts?.TryGetValue(startPageInfo.PageId, out context);
                    StartPageContexts[startPageInfo.PageUniqueId] = context;
                    PageContexts[startPageInfo.PageUniqueId] = context;
                }

                IsStartPagesCreated = true;
            });
        }

        [NotNull]
        private IStoryboardPageView CreatePageView([NotNull] InnerStoryboardPageInfo pageInfo)
        {
            if (!RegisteredPages.ContainsKey(pageInfo))
            {
                throw new InvalidOperationException($"Page creation info with pageTypeId {pageInfo.PageId} not registered");
            }

            var pageCreationInfo = RegisteredPages[pageInfo];
            var view = PageCreator.CreateView(pageCreationInfo.View);
            var viewModel = PageCreator.CreateViewModel(pageCreationInfo.ViewModel);
            viewModel.PageId = pageInfo.PageId;
            viewModel.StoryboardId = pageInfo.StoryboardId;
            viewModel.PageBackRequested += ViewModelOnPageBackRequested;
            viewModel.PageCanceled += ViewModelOnPageCanceled;
            viewModel.PageCompleted += ViewModelOnPageCompleted;
            viewModel.PageTransitionRequested += ViewModelOnPageTransitionRequested;
            view.ViewModel = viewModel;
            CachedPages.Add(pageInfo.PageUniqueId, view);
            return view;
        }

        private async Task<TransitionResult> ViewModelOnPageTransitionRequested(object sender,
            [NotNull] TransitionRequest transitionRequest)
        {
            if (transitionRequest == null) throw new ArgumentNullException(nameof(transitionRequest));

            var tcs = new TaskCompletionSource<TransitionResult>();
            await Invoker.InvokeAsync(async () =>
            {
                try
                {
                    if (!(sender is IStoryboardPageViewModel viewModel))
                        throw new InvalidOperationException("Incorrect request of transition");

                    var result = await GoToPageAsync(
                            transitionRequest.DestinationPageId,
                            viewModel.StoryboardId,
                            transitionRequest.DestinationPageContext)
                        .ConfigureAwait(true);
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    ExceptionOccured?.Invoke(this, e);
                    tcs.SetResult(TransitionResult.Failed);
                }
            }).ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(true);
        }
        

        private Task<TransitionResult> ViewModelOnPageCompleted([NotNull] TransitionEvent transitionEvent)
        {
            if (transitionEvent == null) throw new ArgumentNullException(nameof(transitionEvent));
            if (!(transitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");
            
            return HandleViewModelTransitionsAsync(
                transitionEvent.Sender, 
                PageTransitionTrigger.Completed, 
                transitionEvent.Context);
        }

        protected virtual async Task<TransitionResult> HandleViewModelTransitionsAsync(
            object sender, 
            PageTransitionTrigger trigger, 
            IStoryboardPageContext context = null)
        {
            var tcs = new TaskCompletionSource<TransitionResult>();
            await Invoker.InvokeAsync(async () =>
            {
                try
                {
                    if (!(sender is IStoryboardPageViewModel viewModel))
                        throw new InvalidOperationException("Incorrect request of transition");

                    // todo is it correct? different behaviour of transition
                    var removingResult = await RemoveOpennedPageAsync()
                        .ConfigureAwait(true);
                    if (removingResult == null)
                    {
                        tcs.SetResult(TransitionResult.Unavailable);
                        return;
                    }

                    if (removingResult.Item2 != TransitionResult.Completed)
                    {
                        tcs.SetResult(removingResult.Item2);
                        return;
                    }

                    var storyboard = Storyboards.Values.FirstOrDefault(x => x.StoryboardId == viewModel.StoryboardId);
                    if (storyboard == null) throw new InvalidOperationException("Storyboard not found");

                    var destinationPageId = storyboard.GetDestinationPage(viewModel.PageId, trigger);
                    var result = await GoToPageAsync(destinationPageId, storyboard.StoryboardId, context)
                        .ConfigureAwait(true);

                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    ExceptionOccured?.Invoke(this, e);
                    tcs.SetResult(TransitionResult.Failed);
                }
            }).ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(false);
        }

        private Task<TransitionResult> ViewModelOnPageCanceled([NotNull] TransitionEvent transitionEvent)
        {
            if (transitionEvent == null) throw new ArgumentNullException(nameof(transitionEvent));
            if (!(transitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");
            return HandleViewModelTransitionsAsync(
                transitionEvent.Sender, 
                PageTransitionTrigger.Canceled, 
                transitionEvent.Context);
        }

        private Task<TransitionResult> ViewModelOnPageBackRequested([NotNull] TransitionEvent transitionEvent)
        {
            if (transitionEvent == null) throw new ArgumentNullException(nameof(transitionEvent));
            if (!(transitionEvent.Sender is IStoryboardPageViewModel))
                throw new InvalidOperationException("Incorrect request of transition");

            return HandleViewModelTransitionsAsync(
                transitionEvent.Sender, 
                PageTransitionTrigger.Back, 
                transitionEvent.Context);
        }

        public virtual async Task<TransitionResult> GoToPageAsync(
            Guid pageId, 
            Guid? storyboardId = null, 
            IStoryboardPageContext pageContext = null)
        {
            var tcs = new TaskCompletionSource<TransitionResult>();
            await Invoker.InvokeAsync(async () =>
            {
                try
                {
                    var storyBoard = storyboardId.HasValue
                        ? Storyboards.FirstOrDefault(x => x.Key == storyboardId.Value).Value
                        : ActiveStoryboard;

                    //first try to find desired page in current storyboard, then look up at all registered pages
                    var pageInfo = storyBoard != null
                        ? RegisteredPages.Keys.FirstOrDefault(x =>
                            x.PageId == pageId && x.StoryboardId == storyBoard.StoryboardId)
                        : RegisteredPages.Keys.FirstOrDefault(x => x.PageId == pageId);
                    if (pageInfo == null)
                    {
                        pageInfo = RegisteredPages.Keys.FirstOrDefault(x => x.PageId == pageId);
                        if (pageInfo == null)
                        {
                            throw new InvalidOperationException("Page not found");
                        }
                    }

                    var result = await OpenPageAsync(pageInfo, pageContext)
                        .ConfigureAwait(true);
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    ExceptionOccured?.Invoke(this, e);
                    tcs.SetResult(TransitionResult.Failed);
                }
            }).ConfigureAwait(false);
            return await tcs.Task
                .ConfigureAwait(false);
        }

        protected virtual async Task<TransitionResult> OpenPageAsync(
            [NotNull] InnerStoryboardPageInfo pageInfo,
            [CanBeNull] IStoryboardPageContext pageContext = null, 
            bool addToJournal = true)
        {
            if (!Storyboards.ContainsKey(pageInfo.StoryboardId))
            {
                throw new InvalidOperationException($"Storyboard {pageInfo.StoryboardId} does not registered");
            }

            var tcs = new TaskCompletionSource<TransitionResult>();
            await Invoker.InvokeAsync(async () =>
            {
                try
                {
                    var storyboard = Storyboards[pageInfo.StoryboardId];

                    if (ActiveStoryboard?.ActivePage != null)
                    {
                        var previousPage = ActiveStoryboard.ActivePage;
                        var viewModel = previousPage.ViewModel;

                        var canLeave = await viewModel.CanLeaveAsync().ConfigureAwait(true);
                        if (!canLeave)
                        {
                            tcs.SetResult(TransitionResult.CanceledByUser);
                            return;
                        }

                        await viewModel.LeaveAsync().ConfigureAwait(true);
                    }

                    var storyboardsWasChanged = ActiveStoryboard == null
                                               || ActiveStoryboard.StoryboardId != storyboard.StoryboardId;
                    ActiveInnerStoryboardPageInfo = pageInfo;
                    ActiveStoryboard = storyboard;

                    if (storyboardsWasChanged)
                    {
                        ActiveStoryboardChanged?.Invoke(this, pageInfo.StoryboardId);
                    }

                    if (CachedPages.ContainsKey(pageInfo.PageUniqueId))
                    {
                        var page = CachedPages[pageInfo.PageUniqueId];

                        storyboard.ActivePage = page;
                        PageContexts.TryGetValue(pageInfo.PageUniqueId, out var restoredPageContext);

                        StartPagesOpenningStat.TryGetValue(pageInfo.PageUniqueId, out var wasPageOpenned);
                        if (wasPageOpenned)
                        {
                            await page.ViewModel.ReturnAsync(pageContext ?? restoredPageContext).ConfigureAwait(true);
                        }
                        else
                        {
                            await page.ViewModel.OpenAsync(pageContext ?? restoredPageContext).ConfigureAwait(true);
                            StartPagesOpenningStat[pageInfo.PageUniqueId] = true;
                        }

                    }
                    else
                    {
                        var view = CreatePageView(pageInfo);
                        storyboard.ActivePage = view;

                        await view.ViewModel.OpenAsync(pageContext).ConfigureAwait(true);
                        PageContexts[pageInfo.PageUniqueId] = pageContext;
                    }

                    if (addToJournal)
                    {
                        Journal.AddLast(pageInfo);
                    }

                    tcs.SetResult(TransitionResult.Completed);
                    CanBackChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    ExceptionOccured?.Invoke(this, e);
                    tcs.SetResult(TransitionResult.Failed);
                }
               
            }).ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(false);
        }
       
        public virtual Task<TransitionResult> GoToStoryboardAsync(Guid storyboardId)
        {
            // because page have already been added to journal
            var addPageToJournal = false;
            var pageInfo = Journal.LastOrDefault(x => x.StoryboardId == storyboardId);
            if (pageInfo == null)
            {
                pageInfo =
                    RegisteredPages.Keys.FirstOrDefault(x => x.StoryboardId == storyboardId && x.IsStartPage);
                if (pageInfo == null) throw new InvalidOperationException("Pages for storyboard does not registered");
                addPageToJournal = true;
            }
            return OpenPageAsync(
                pageInfo, 
                addToJournal: addPageToJournal);
        }
        

        public virtual async Task<TransitionResult> GoBackAsync()
        {
            if (!CanGoBack()) return TransitionResult.Unavailable;
            if (ActiveInnerStoryboardPageInfo == null) return TransitionResult.Unavailable;

            var tcs = new TaskCompletionSource<TransitionResult>();
            await Invoker.InvokeAsync(async () =>
            {
                try
                {
                    var lastPageFromStoryboard = await RemoveOpennedPageAsync().ConfigureAwait(true);
                    if (lastPageFromStoryboard == null)
                    {
                        tcs.SetResult(TransitionResult.Unavailable);
                        return;
                    }

                    if (lastPageFromStoryboard.Item2 != TransitionResult.Completed)
                    {
                        tcs.SetResult(lastPageFromStoryboard.Item2);
                        return;
                    }

                    // already have been added
                    var result = await OpenPageAsync(lastPageFromStoryboard.Item1, addToJournal: false)
                        .ConfigureAwait(true);
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    ExceptionOccured?.Invoke(this, e);
                    tcs.SetResult(TransitionResult.Failed);
                }

            }).ConfigureAwait(false);
            return await tcs.Task;
        }

        protected virtual async Task<Tuple<InnerStoryboardPageInfo, TransitionResult>> RemoveOpennedPageAsync()
        {
            if (ActiveInnerStoryboardPageInfo == null) return null;

            // start pages does not close
            if (ActiveStoryboard?.ActivePage != null && 
                !ActiveInnerStoryboardPageInfo.IsStartPage)
            {
                var previousPage = ActiveStoryboard.ActivePage;
                var viewModel = previousPage.ViewModel;
                var canClose = await viewModel.CanCloseAsync().ConfigureAwait(true);
                if (!canClose)
                {
                    return new Tuple<InnerStoryboardPageInfo, TransitionResult>(null, TransitionResult.CanceledByUser);
                }
            }
            
            var lastPageFromStoryboard = GetLastPageFromStoryboard();

            if (lastPageFromStoryboard == null)
            {
                return new Tuple<InnerStoryboardPageInfo, TransitionResult>(null, TransitionResult.Unavailable);
            }

            await RemovePageAsync(ActiveInnerStoryboardPageInfo, ActiveStoryboard)
                .ConfigureAwait(true);

            return new Tuple<InnerStoryboardPageInfo, TransitionResult>(lastPageFromStoryboard, TransitionResult.Completed);
        }

        protected virtual async Task RemovePageAsync(InnerStoryboardPageInfo pageInfo, Storyboard storyboard)
        {
            Journal.Remove(pageInfo);

            //start pages always in memory
            if (CachedPages.ContainsKey(pageInfo.PageUniqueId) &&
                !pageInfo.IsStartPage)
            {
                CachedPages.Remove(pageInfo.PageUniqueId);

                if (storyboard?.ActivePage != null)
                {
                    var previousPage = storyboard.ActivePage;
                    var viewModel = previousPage.ViewModel;
                    // can close called early

                    viewModel.PageBackRequested -= ViewModelOnPageBackRequested;
                    viewModel.PageCanceled -= ViewModelOnPageCanceled;
                    viewModel.PageCompleted -= ViewModelOnPageCompleted;
                    viewModel.PageTransitionRequested -= ViewModelOnPageTransitionRequested;

                    await viewModel.CloseAsync().ConfigureAwait(true);
                    storyboard.ActivePage = null;
                    viewModel.Dispose();
                }
            }

            if (PageContexts.ContainsKey(pageInfo.PageUniqueId))
            {
                PageContexts.Remove(pageInfo.PageUniqueId);
            }

            //todo maybe add isShared flag?
            if (pageInfo.IsStartPage &&
                StartPageContexts.ContainsKey(pageInfo.PageUniqueId))
            {
                PageContexts[pageInfo.PageUniqueId] =
                    StartPageContexts[pageInfo.PageUniqueId];
            }
        }

        protected virtual InnerStoryboardPageInfo GetLastPageFromStoryboard()
        {
            var lastPageFromStoryboard = ActiveInnerStoryboardPageInfo.IsStartPage
                ? Journal.LastOrDefault(x =>
                    x.StoryboardId != ActiveInnerStoryboardPageInfo.StoryboardId)
                : Journal.LastOrDefault(x =>
                    x.StoryboardId == ActiveInnerStoryboardPageInfo.StoryboardId
                    && x.PageId != ActiveInnerStoryboardPageInfo.PageId);

            return
                lastPageFromStoryboard ??
                Journal.LastOrDefault(x => x.StoryboardId != ActiveInnerStoryboardPageInfo.StoryboardId);
        }

        public virtual bool CanGoBack()
        {
            return Invoker.Invoke(() => {
                if (ActiveStoryboard?.ActivePage == null) return false;
                if (Storyboards.Count < 0) return false;
                // if no page or one there is no reason to execute back command
                if (Journal.Count < 2) return false;
                // if opened only start pages there is no reason to execute back command
                if (Journal.All(x => x.IsStartPage)) return false;

                return true;
            });
        }

        public virtual async Task<TransitionResult> GoToStoryboardStartPage(Guid storyboardId)
        {
            if (!Storyboards.ContainsKey(storyboardId))
                throw new ArgumentException($"Storyboard with Id {storyboardId} not registered");

            var tcs = new TaskCompletionSource<TransitionResult>();

            await Invoker.InvokeAsync(async () =>
                {
                    var previousActiveStoryboard = ActiveStoryboard;
                    try
                    {
                        if (ActiveStoryboard?.ActivePage != null)
                        {
                            var previousPage = ActiveStoryboard.ActivePage;
                            var viewModel = previousPage.ViewModel;

                            var canLeave = await viewModel.CanLeaveAsync().ConfigureAwait(true);
                            if (!canLeave)
                            {
                                tcs.SetResult(TransitionResult.CanceledByUser);
                                return;
                            }

                            await viewModel
                                .LeaveAsync().ConfigureAwait(true);
                        }
                        
                        var pagesToClose = Journal
                            .Where(x => x.StoryboardId == storyboardId)
                            .ToList();

                        ActiveStoryboard = null;
                        switch (pagesToClose.Count)
                        {
                            case 0:
                            {
                                var startPage =
                                    RegisteredPages.Keys.FirstOrDefault(
                                        x => x.StoryboardId == storyboardId && x.IsStartPage);
                                if (startPage == null)
                                    throw new InvalidOperationException("Pages for storyboard does not registered");

                                var result = await OpenPageAsync(startPage).ConfigureAwait(true);
                                tcs.SetResult(result);
                                return;
                            }
                            case 1:
                            {
                                var result = await OpenPageAsync(
                                        pagesToClose[0],
                                        addToJournal: false)
                                    .ConfigureAwait(true);
                                tcs.SetResult(result);
                                return;
                            }
                            default:
                            {
                                var orderedPagesToClose = new Stack<InnerStoryboardPageInfo>(pagesToClose);

                                var storyboard = Storyboards[storyboardId];
                                var pageInfo = orderedPagesToClose.Pop();
                                while (!pageInfo.IsStartPage && orderedPagesToClose.Count > 0)
                                {
                                    if (!CachedPages.ContainsKey(pageInfo.PageUniqueId)) 
                                        throw new InvalidOperationException($"Page with Id {pageInfo.PageUniqueId} not cached");

                                    var page = CachedPages[pageInfo.PageUniqueId];

                                    var canClose = await page.ViewModel
                                        .CanCloseAsync()
                                        .ConfigureAwait(false);
                                    if (!canClose)
                                    {
                                        // open page that user don't want to close
                                        var openningResult = await OpenPageAsync(
                                                pageInfo,
                                                addToJournal: false)
                                            .ConfigureAwait(true);

                                        tcs.SetResult(openningResult == TransitionResult.Completed
                                            ? TransitionResult.CanceledByUser // because user canceled transition to start page
                                            : openningResult);
                                        return;
                                    }

                                    await RemovePageAsync(
                                            pageInfo, 
                                            storyboard)
                                        .ConfigureAwait(true);
                                    
                                    pageInfo = orderedPagesToClose.Pop();
                                }
                                
                                var result = await OpenPageAsync(
                                        pageInfo,
                                        addToJournal: false)
                                    .ConfigureAwait(true);
                                tcs.SetResult(result);
                                return;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        tcs.SetResult(TransitionResult.Failed);
                        ExceptionOccured?.Invoke(tcs, e);
                        ActiveStoryboard = previousActiveStoryboard;
                    }

                })
                .ConfigureAwait(false);
            return await tcs.Task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            Invoker.Invoke(() =>
            {
                foreach (var page in CachedPages)
                {
                    var viewModel = page.Value?.ViewModel;
                    viewModel?.Dispose();
                }
            });
        }

    }
}