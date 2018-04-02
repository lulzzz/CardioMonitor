using System;
using System.Linq;
using JetBrains.Annotations;
using Markeli.Storyboards;
using SimpleInjector;

namespace CardioMonitor.Ui
{
    public class SimpleInjectorPageCreator : IStoryboardPageCreator
    {
        private readonly Container _container;

        public SimpleInjectorPageCreator([NotNull] Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IStoryboardPageView CreateView([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!type.GetInterfaces().Contains(typeof(IStoryboardPageView)))
                throw new InvalidOperationException($"type must implement {nameof(IStoryboardPageView)}");

            // cause we should not register views in IoC
            return Activator.CreateInstance(type) as IStoryboardPageView;
        }

        public IStoryboardPageViewModel CreateViewModel([NotNull] Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!type.GetInterfaces().Contains(typeof(IStoryboardPageViewModel)))
                throw new InvalidOperationException($"type must implement {nameof(IStoryboardPageViewModel)}");

            return _container.GetInstance(type) as IStoryboardPageViewModel;
        }
    }
}