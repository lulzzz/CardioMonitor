using System;
using JetBrains.Annotations;
using Markeli.Storyboards;

namespace CardioMonitor.Ui.ViewModel
{
    

    public class ExtendedStoryboard : Storyboard
    {
        public string Name
        {
            get => _name;
            set
            {
                if (!Equals(value, _name))
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string _name;
        
        public object Icon
        {
            get => _icon;
            set
            {
                if (!Equals(_icon, value))
                {
                    _icon = value;
                    OnPropertyChanged(nameof(Icon));
                } 

            }
        }
        private object _icon;

        public ExtendedStoryboard(
            Guid storyboardId,
            [NotNull] string name,
            [NotNull] object icon) : base(storyboardId)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Icon = icon ?? throw new ArgumentNullException(nameof(icon));
        }
    }
}