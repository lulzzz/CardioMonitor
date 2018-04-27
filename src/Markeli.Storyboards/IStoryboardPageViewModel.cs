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

        event Func<TransitionEvent, Task> PageCanceled;

        event Func<TransitionEvent, Task> PageCompleted;

        event Func<TransitionEvent, Task> PageBackRequested;

        event Func<object, TransitionRequest, Task> PageTransitionRequested;
    }
}
