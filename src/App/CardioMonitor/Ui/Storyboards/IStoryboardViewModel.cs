﻿using System;
using JetBrains.Annotations;

namespace CardioMonitor.Ui.Storyboards
{
    public interface IStoryboardViewModel : IDisposable
    {
        Guid PageId { get; set; }

        Guid StoryboardId { get; set; }

        void Open([CanBeNull] IPageContext context);

        bool CanLeave();

        void Leave();
        
        void Return([CanBeNull] IPageContext context);

        bool CanClose();

        void Close();

        event EventHandler PageCanceled;

        event EventHandler PageCompleted;

        event EventHandler PageBackRequested;

        event EventHandler<TransitionRequest> PageTransitionRequested;
    }
}