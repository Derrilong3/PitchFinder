using PitchFinder.ViewModels;
using System.Windows.Media;

namespace PitchFinder.Models
{
    internal class NoteBox : ViewModelBase
    {
        public string Text { get; set; }

        private Color _color;
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

    }
}
