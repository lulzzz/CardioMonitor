using System;
using System.Threading.Tasks;

namespace Markeli.Storyboards
{
    public static class StoryboardEventAsyncHandlerExtensions
    {
        public static Task InvokeAsync(this Func<object, Task> handler, object arg)
        {
            if (handler == null)
            {
                return Task.CompletedTask;
            }

            var invocationList = handler.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, Task>)invocationList[i])(arg);
            }

            return Task.WhenAll(handlerTasks);
        }

        public static Task InvokeAsync(this Func<object, TransitionRequest, Task> handler, object arg, TransitionRequest request)
        {
            if (handler == null)
            {
                return Task.CompletedTask;
            }

            var invocationList = handler.GetInvocationList();
            var handlerTasks = new Task[invocationList.Length];

            for (var i = 0; i < invocationList.Length; i++)
            {
                handlerTasks[i] = ((Func<object, TransitionRequest, Task>)invocationList[i])(arg, request);
            }

            return Task.WhenAll(handlerTasks);
        }
    }
}