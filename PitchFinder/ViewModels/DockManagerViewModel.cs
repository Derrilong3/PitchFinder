using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PitchFinder.ViewModels
{
    public class DockManagerViewModel
    {
        /// <summary>Gets a collection of all visible documents</summary>
        public ObservableCollection<DockWindowViewModel> Documents { get; private set; }

        public ObservableCollection<DockWindowViewModel> Anchorables { get; private set; }

        public DockManagerViewModel(IEnumerable<DockWindowViewModel> dockWindowViewModels)
        {
            this.Documents = new ObservableCollection<DockWindowViewModel>();
            this.Anchorables = new ObservableCollection<DockWindowViewModel>();

            foreach (var document in dockWindowViewModels)
            {
                document.PropertyChanged += DockWindowViewModel_PropertyChanged;
                if (!document.IsClosed)
                    this.Anchorables.Add(document);
            }
        }

        private void DockWindowViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DockWindowViewModel document = sender as DockWindowViewModel;

            if (e.PropertyName == nameof(DockWindowViewModel.IsClosed))
            {
                if (!document.IsClosed)
                    this.Anchorables.Add(document);
                else
                    this.Anchorables.Remove(document);
            }
        }
    }


}
