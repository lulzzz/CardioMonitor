using System;
using System.Windows.Threading;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    public interface IStoryboardPageCreator
    {
        IStoryboardPageView CreateView(Type type);

        IStoryboardPageViewModel CreateViewModel(Type type);
    }

    public interface IUiInvoker
    {
        void Invoke(Action action);
    }
}