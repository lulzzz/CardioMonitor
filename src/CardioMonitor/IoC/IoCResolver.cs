using System;
using CardioMonitor.Properties;
using SimpleInjector;

namespace CardioMonitor.IoC
{
    public static class IoCResolver
    {
        public static bool IsInitialized { get; private set; }

        [CanBeNull]
        private static Container _container;


        public static void SetContainer(Container container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (IsInitialized) throw new InvalidOperationException("Класс уже проинициализирован.");

            _container = container;
            IsInitialized = true;
        }

        public static void Reset()
        {
            _container = null;
        }

        public static T Resolve<T>()  where T: class
        {
            if (!IsInitialized) throw new InvalidOperationException("Класс не проинициализирован.");

            var instance = _container.GetInstance(typeof(T)) as T;
            return instance;
        }
    }
}