using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public class Storyboard : INotifyPropertyChanged
    {

        private readonly Dictionary<Guid, StoryboardPageInfo> _registeredPages;
        private readonly Dictionary<Guid, List<TransitionInfo>> _transitions;

        public IStoryboardPageView ActivePage
        {
            get => _activePage;
            set
            {
                if (Equals(_activePage, value)) return;

                OnPropertyChanged(nameof(ActivePage));
                _activePage = value; 

            }
        }
        private IStoryboardPageView _activePage;


        public Guid StoryboardId { get; }

        public Storyboard(Guid storyboardId)
        {
            StoryboardId = storyboardId;
            _registeredPages = new Dictionary<Guid, StoryboardPageInfo>();
            _transitions = new Dictionary<Guid, List<TransitionInfo>>();
        }

        public void RegisterPage(
            Guid pageId,
            bool isStartPage,
            [NotNull] Type view,
            [NotNull] Type viewModel)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            if (_registeredPages.ContainsKey(pageId)) throw new InvalidOperationException("Page with same ID have been already registered");

            _registeredPages[pageId] = new StoryboardPageInfo(
                pageId,
                isStartPage,
                view,
                viewModel);
        }

        [NotNull]
        internal ICollection<StoryboardPageInfo> GetPageInfos()
        {
            return _registeredPages.Values;
        }

        public void RegisterTransition(Guid sourcePageId, Guid destinationPageId,
            PageTransitionTrigger transitionTrigger)
        {
            if (!_registeredPages.ContainsKey(sourcePageId)) throw new InvalidOperationException("Page with same ID have not been registered");

            if (!_transitions.ContainsKey(sourcePageId))
            {
                _transitions[sourcePageId] = new List<TransitionInfo>(1);
            }
            _transitions[sourcePageId].Add(new TransitionInfo
            {
                DestinationId = destinationPageId,
                Trigger = transitionTrigger
            });
        }

        public Guid GetDestinationPage(Guid sourcePageId, PageTransitionTrigger trigger)
        {
            if (!_transitions.ContainsKey(sourcePageId))
            {
                throw new ArgumentException("No transitions for page");
            }

            var destination = _transitions[sourcePageId].FirstOrDefault(x => x.Trigger == trigger);
            if (destination == null)
            {
                throw new InvalidOperationException("No transition for trigger");
            }

            return destination.DestinationId;
        }

        private class TransitionInfo
        {
            public Guid DestinationId { get; set; }

            public PageTransitionTrigger Trigger { get; set; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}