using System;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public class TransitionRequest
    {
        public TransitionRequest(
            Guid destinationPageId, 
            [CanBeNull] IStoryboardPageContext destinationPageContext)
        {
            DestinationPageId = destinationPageId;
            DestinationPageContext = destinationPageContext;
        }

        public Guid DestinationPageId { get; }

        [CanBeNull]
        public IStoryboardPageContext DestinationPageContext { get;  }
    }
}