using System;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public interface IStoryboardPageViewModel : IDisposable
    {
        Guid PageId { get; set; }

        Guid StoryboardId { get; set; }

        void Open([CanBeNull] IStoryboardPageContext context);

        bool CanLeave();

        void Leave();
        
        void Return([CanBeNull] IStoryboardPageContext context);

        bool CanClose();

        void Close();

        event EventHandler PageCanceled;

        event EventHandler PageCompleted;

        event EventHandler PageBackRequested;

        event EventHandler<TransitionRequest> PageTransitionRequested;
        
        event EventHandler CanCloseChanged;

        event EventHandler CanLeaveChanged;
    }
}
