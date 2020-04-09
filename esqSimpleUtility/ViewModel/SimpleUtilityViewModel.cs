using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using esqSimpleUtility.Model;
using esqSimpleUtility.Services;

namespace esqSimpleUtility.ViewModel
{
    public class SimpleUtilityViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private ObservableCollection<NameValuePair> _listViewItems;
        public ObservableCollection<NameValuePair> ListViewItems
        {
            get => _listViewItems;
            set => Set(ref _listViewItems, value);
        }

        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                ValidateInputText(value);
            }
        }

        private string _statusBarCount;
        public string StatusBarCount
        {
            get => _statusBarCount;
            set => Set(ref _statusBarCount, value);
        }

        private string _statusBarSort;
        public string StatusBarSort
        {
            get => _statusBarSort;
            set => Set(ref _statusBarSort, value);
        }

        private string _statusBarFilter;
        public string StatusBarFilter
        {
            get => _statusBarFilter;
            set => Set(ref _statusBarFilter, value);
        }

        public ICommand AddCommand { get; private set; }         // CommandParameter: input text
        public ICommand SortCommand { get; private set; }        // CommandParameter: sort mode
        public ICommand DeleteCommand { get; private set; }      // CommandParameter: selected items
        public ICommand ApplyFilterCommand { get; private set; } // CommandParameter: input text
        public ICommand ClearFilterCommand { get; private set; } // Without CommandParameter

        // SortMode and FilterMode options
        public enum Mode
        {
            None = 0,
            ByName = 1,
            ByValue = 2,
        }

        private Mode currentSortMode = Mode.None;
        private Mode currentFilterMode = Mode.None;
        private string currentFilterText = string.Empty;
        private Predicate<object> currentFilter = null;

        private IDataService dataService;

        public SimpleUtilityViewModel(IDataService dataService)
        {
            this.dataService = dataService;

            AddCommand = new RelayCommand<string>(AddNew, (input) => !HasErrors);
            DeleteCommand = new RelayCommand<IList<object>>(DeleteSelected, (selected) => selected?.Count > 0);

            SortCommand = new RelayCommand<string>(SetSortMode);
            ApplyFilterCommand = new RelayCommand<string>(SetFilter, (input) => IsValidFilterType(input));
            ClearFilterCommand = new RelayCommand(ClearFilter, () => currentFilterMode != Mode.None);

            InputText = "name = value";
            FetchItems();
        }

        private void UpdateStatusBar()
        {
            StatusBarCount = $"Count = {ListViewItems?.Count}";
            StatusBarSort = $"Sort = {currentSortMode}";

            if (currentFilterMode != Mode.None)
                StatusBarFilter = $"Filter = {currentFilterMode} \"{currentFilterText}\"";
            else
                StatusBarFilter = $"Filter = {currentFilterMode}";
        }

        private void FetchItems()
        {
            ListViewItems = new ObservableCollection<NameValuePair>(dataService.GetAll());
            UpdateStatusBar();
        }

        private void AddNew(string input)
        {
            if (NameValuePair.TryParse(input, out var pair))
            {
                if (dataService.Add(pair))
                    ListViewItems.Add(pair);

                UpdateStatusBar();
            }
        }

        private void DeleteSelected(IList<object> param)
        {
            if (param?.Count > 0)
            {
                var selectedItems = param.Cast<NameValuePair>().ToList();

                foreach (var item in selectedItems)
                {
                    if (dataService.Delete(item.Id))
                        ListViewItems.Remove(item);
                }

                UpdateStatusBar();
            }
        }

        private void SetSortMode(string param)
        {
            if (Enum.TryParse(param, out Mode mode))
            {
                if (currentSortMode != mode)
                {
                    currentSortMode = mode;
                    ApplySortMode();
                    UpdateStatusBar();
                }
            }
        }

        private bool IsValidFilterType(string input)
        {
            if (NameValuePair.TryParse(input, out var pair))
                return (pair.Name == "Name" || pair.Name == "Value");

            return false;
        }

        private void SetFilter(string input)
        {
            if (NameValuePair.TryParse(input, out var pair))
            {
                string filterType = pair.Name;
                switch (filterType)
                {
                    case "Name":
                        currentFilterMode = Mode.ByName;
                        currentFilterText = pair.Value;
                        currentFilter = (obj) => (obj is NameValuePair x && x.Name == currentFilterText);
                        break;
                    case "Value":
                        currentFilterMode = Mode.ByValue;
                        currentFilterText = pair.Value;
                        currentFilter = (obj) => (obj is NameValuePair x && x.Value == currentFilterText);
                        break;
                    default:
                        currentFilterMode = Mode.None;
                        currentFilterText = "";
                        currentFilter = null;
                        break;
                }

                ApplyFilter();
                UpdateStatusBar();
            }
        }

        private void ClearFilter()
        {
            currentFilterMode = Mode.None;
            currentFilterText = "";
            currentFilter = null;

            ApplyFilter();
            UpdateStatusBar();
        }

        private void ApplySortMode()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ListViewItems);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                switch (currentSortMode)
                {
                    case Mode.ByName:
                        view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                        break;
                    case Mode.ByValue:
                        view.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
                        break;
                    default:
                        break;
                }
            }
        }

        private void ApplyFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(ListViewItems);
            if (view != null)
                view.Filter = currentFilter;
        }

        // ViewModelBase in MVVM Light Toolkit doesn't provide INotifyDataErrorInfo functionality.
        // TODO: find a more elegant solution. In the meantime, for the purpose of the test task:
        #region INotifyDataErrorInfo members
        private readonly Dictionary<string, ICollection<string>>
            _validationErrors = new Dictionary<string, ICollection<string>>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)
                || !_validationErrors.ContainsKey(propertyName))
                return null;

            return _validationErrors[propertyName];
        }

        public bool HasErrors
        {
            get { return _validationErrors.Count > 0; }
        }

        private void ValidateInputText(string input)
        {
            const string propertyKey = "InputText";
            bool isValid = NameValuePair.TryParse(input, out var obj);
            if (isValid)
            {
                _validationErrors.Remove(propertyKey);
                RaiseErrorsChanged(propertyKey);
            }
            else
            {
                List<string> validationErrors = new List<string>()
                {
                    "Please enter a valid Name/Value pair.",
                };
                _validationErrors[propertyKey] = validationErrors;
                RaiseErrorsChanged(propertyKey);
            }
        }
        #endregion
    }
}
