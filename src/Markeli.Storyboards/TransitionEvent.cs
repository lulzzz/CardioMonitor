using System;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public class TransitionEvent
    {
        public TransitionEvent(
            [NotNull] object sender, 
            [CanBeNull] IStoryboardPageContext context = null)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Context = context;
        }

        [NotNull]
        public object Sender { get; }

        [CanBeNull]
        public IStoryboardPageContext Context { get; }
    }
}