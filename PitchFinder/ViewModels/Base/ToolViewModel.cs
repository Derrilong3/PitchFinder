namespace PitchFinder.ViewModels
{
    public abstract class ToolViewModel : ViewModelBase
    {
        private string _title;
        private bool _isVisible = false;
        private string _contentId = null;
        private bool _isSelected = false;
        private bool _isActive = false;

        public ToolViewModel(string name)
        {
            Title = name;
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string ContentId
        {
            get => _contentId;
            set
            {
                if (_contentId != value)
                {
                    _contentId = value;
                    OnPropertyChanged(nameof(ContentId));
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnVisibilityChanged();
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }
        
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        protected abstract void OnVisibilityChanged();
    }
}
