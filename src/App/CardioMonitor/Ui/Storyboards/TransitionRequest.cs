using System;
using JetBrains.Annotations;

namespace CardioMonitor.Ui.Storyboards
{
    public class TransitionRequest
    {
        public TransitionRequest(
            Guid destinationPageId, 
            [CanBeNull] IPageContext destinationPageContext)
        {
            DestinationPageId = destinationPageId;
            DestinationPageContext = destinationPageContext;
        }

        public Guid DestinationPageId { get; }

        [CanBeNull]
        public IPageContext DestinationPageContext { get;  }
    }
}