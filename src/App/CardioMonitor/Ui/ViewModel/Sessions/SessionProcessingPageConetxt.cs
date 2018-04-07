﻿using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingPageConetxt : IStoryboardPageContext
    {
        public int PatientId { get; set; }

        public bool IsAutopumpingEnabled { get; set; }

        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        public float MaxAngleX { get; set; }

        /// <summary>
        /// Количество циклов (повторений)
        /// </summary>
        public short CyclesCount { get; set; }

        /// <summary>
        /// Частота движения
        /// </summary>
        public float MovementFrequency { get; set; }


        public short PumpingNumberOfAttemptsOnStartAndFinish { get; set; }

        public short PumpingNumberOfAttemptsOnProcessing { get; set; }
    }
}