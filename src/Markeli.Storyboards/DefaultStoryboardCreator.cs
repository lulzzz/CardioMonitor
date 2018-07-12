using System;
using System.Linq;
using JetBrains.Annotations;

namespace Markeli.Storyboards
{
    internal class DefaultStoryboardCreator : IStoryboardPageCreator
    {
        
        public IStoryboardPageView CreateView([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!type.GetInterfaces().Contains(typeof(IStoryboardPageView)))
                throw new InvalidOperationException($"type must implement {nameof(IStoryboardPageView)}");

            return Activator.CreateInstance(type) as IStoryboardPageView;
        }

        public IStoryboardPageViewModel CreateViewModel([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!type.GetInterfaces().Contains(typeof(IStoryboardPageViewModel)))
                throw new InvalidOperationException($"type must implement {nameof(IStoryboardPageViewModel)}");

            return Activator.CreateInstance(type) as IStoryboardPageViewModel;
        }
    }
}
