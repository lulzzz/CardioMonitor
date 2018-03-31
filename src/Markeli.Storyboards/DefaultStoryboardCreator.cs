using System;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    internal class DefaultStoryboardCreator : IStroryboardPageCreator
    {
        public IStoryboardPageView CreateView([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Activator.CreateInstance(type) as IStoryboardPageView;
        }

        public IStoryboardPageViewModel CreateViewModel([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Activator.CreateInstance(type) as IStoryboardPageViewModel;
        }
    }
}
