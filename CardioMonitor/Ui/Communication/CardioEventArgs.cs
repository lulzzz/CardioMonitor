using System;

namespace CardioMonitor.ViewModel.Communication
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
