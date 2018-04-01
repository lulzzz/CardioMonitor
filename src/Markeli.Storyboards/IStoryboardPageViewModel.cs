using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public interface IStoryboardPageViewModel : IDisposable
    {
        Guid PageId { get; set; }

        Guid StoryboardId { get; set; }

        Task OpenAsync([CanBeNull] IStoryboardPageContext context);

        Task<bool> CanLeaveAsync();

        Task LeaveAsync();
        
        Task ReturnAsync([CanBeNull] IStoryboardPageContext context);

        Task<bool> CanCloseAsync();

        Task CloseAsync();

        event EventHandler PageCanceled;

        event EventHandler PageCompleted;

        event EventHandler PageBackRequested;

        event EventHandler<TransitionRequest> PageTransitionRequested;
    }
}
