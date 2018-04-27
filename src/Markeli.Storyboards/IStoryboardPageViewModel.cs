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

        event Func<TrasitionEvent, Task> PageCanceled;

        event Func<TrasitionEvent, Task> PageCompleted;

        event Func<TrasitionEvent, Task> PageBackRequested;

        event Func<TrasitionEvent, TransitionRequest, Task> PageTransitionRequested;
    }
}
