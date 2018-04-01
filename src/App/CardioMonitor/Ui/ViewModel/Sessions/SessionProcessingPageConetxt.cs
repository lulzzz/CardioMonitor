using CardioMonitor.BLL.CoreContracts.Patients;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel.Sessions
{
    public class SessionProcessingPageConetxt : IStoryboardPageContext
    {
        public Patient Patient { get; set; }

        public bool IsAutopumpingEnabled { get; set; }

        /// <summary>
        /// Максимальный угол кровати по оси Х, до которой она будет подниматься
        /// </summary>
        public double MaxAngleX { get; set; }

        /// <summary>
        /// Количество циклов (повторений)
        /// </summary>
        public short CyclesCount { get; set; }

        /// <summary>
        /// Частота движения
        /// </summary>
        public float MovementFrequency { get; set; }
    }
}