using System;

namespace CardioMonitor.Ui.Communication
{
    public class CardioEventArgs : EventArgs
    {
        public int Id { get; set; }

        public CardioEventArgs(int id)
        {
            Id = id;
        }
    }
}
