using CommonServiceLocator;
using GalaSoft.MvvmLight;

namespace esqSimpleUtility.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            CurrentViewModel = ServiceLocator.Current.GetInstance<SimpleUtilityViewModel>();
        }

        // Allows to change ViewModel, not used in this test task.
        private ViewModelBase _CurrentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get { return _CurrentViewModel; }
            set { Set(ref _CurrentViewModel, value); }
        }
    }
}
