﻿using System;
using System.Threading.Tasks;
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

        T Invoke<T>(Func<T> function);

        Task InvokeAsync(Action action);

        Task InvokeAsync(Func<Task> function);
    }
}