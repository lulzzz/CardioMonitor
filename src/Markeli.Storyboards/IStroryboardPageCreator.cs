using System;

namespace Markeli.Storyboards
{
    public interface IStroryboardPageCreator
    {
        IStoryboardPageView CreateView(Type type);

        IStoryboardPageViewModel CreateViewModel(Type type);
    }
}