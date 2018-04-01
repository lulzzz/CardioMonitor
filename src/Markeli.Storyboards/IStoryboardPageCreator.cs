using System;

namespace Markeli.Storyboards
{
    public interface IStoryboardPageCreator
    {
        IStoryboardPageView CreateView(Type type);

        IStoryboardPageViewModel CreateViewModel(Type type);
    }
}