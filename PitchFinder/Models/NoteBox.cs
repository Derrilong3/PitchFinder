using PitchFinder.ViewModels;
using System.Windows.Media;

namespace PitchFinder.Models
{
    internal class NoteBox : ViewModelBase
    {
        public string Text { get; set; }

        private Color color;
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                OnPropertyChanged(nameof(Color));
            }
        }

    }
}
