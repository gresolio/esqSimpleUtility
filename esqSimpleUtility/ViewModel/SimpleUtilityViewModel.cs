using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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
        private enum Mode
        {
            None = 0,
            ByName = 1,
            ByValue = 2,
        }

        private IDataService dataService;
        private List<NameValuePair> lastKnownData;

        private Mode currentSortMode = Mode.None;
        private Mode currentFilterMode = Mode.None;
        private string currentFilter = string.Empty;

        public SimpleUtilityViewModel(IDataService dataService)
        {
            this.dataService = dataService;

            AddCommand = new RelayCommand<string>(AddNew, (input) => !HasErrors);
            DeleteCommand = new RelayCommand<IList<object>>(DeleteSelected, (selected) => selected?.Count > 0);

            SortCommand = new RelayCommand<string>(SetSortMode);
            ApplyFilterCommand = new RelayCommand<string>(ApplyFilter, (input) => !HasErrors);
            ClearFilterCommand = new RelayCommand(ClearFilter, () => currentFilterMode != Mode.None);

            InputText = "name = value";
            FetchItems();
        }

        private void UpdateListView()
        {
            IEnumerable<NameValuePair> items;

            switch (currentFilterMode)
            {
                case Mode.ByName:
                    items = lastKnownData.Where(x => x?.Name == currentFilter);
                    break;
                case Mode.ByValue:
                    items = lastKnownData.Where(x => x?.Value == currentFilter);
                    break;
                default:
                    items = lastKnownData;
                    break;
            }

            ListViewItems = new ObservableCollection<NameValuePair>(items);
        }

        private void UpdateStatusBar()
        {
            StatusBarCount = $"Count = {ListViewItems?.Count}";
            StatusBarSort = $"Sort = {currentSortMode}";

            if (currentFilterMode != Mode.None)
                StatusBarFilter = $"Filter = {currentFilterMode} \"{currentFilter}\"";
            else
                StatusBarFilter = $"Filter = {currentFilterMode}";
        }

        private void FetchItems()
        {
            lastKnownData = dataService.GetAll();
            SortFetchedData();
            UpdateListView();
            UpdateStatusBar();
        }

        private void AddNew(string input)
        {
            if (NameValuePair.TryParse(input, out var pair))
            {
                dataService.Add(pair);
                FetchItems();
            }
        }

        private void DeleteSelected(IList<object> param)
        {
            if (param.Count > 0)
            {
                var selectedItems = param.Cast<NameValuePair>();

                foreach (var item in selectedItems)
                    dataService.Delete(item.Id);

                FetchItems();
            }
        }

        private void SortFetchedData()
        {
            if (lastKnownData == null || currentSortMode == Mode.None)
                return;

            lastKnownData.Sort((lhs, rhs) =>
            {
                switch (currentSortMode)
                {
                    case Mode.ByName:
                        return lhs.Name.CompareTo(rhs.Name);
                    case Mode.ByValue:
                        return lhs.Value.CompareTo(rhs.Value);
                    default:
                        return lhs.Name.CompareTo(rhs.Name);
                }
            });
        }

        private void SetSortMode(string param)
        {
            if (Enum.TryParse(param, out Mode mode))
            {
                if (currentSortMode != mode)
                {
                    currentSortMode = mode;
                    SortFetchedData();
                    UpdateListView();
                    UpdateStatusBar();
                }
            }
        }

        private void ApplyFilter(string input)
        {
            if (NameValuePair.TryParse(input, out var pair))
            {
                string filterType = pair.Name;

                switch (filterType)
                {
                    case "Name":
                        currentFilterMode = Mode.ByName;
                        currentFilter = pair.Value;
                        break;
                    case "Value":
                        currentFilterMode = Mode.ByValue;
                        currentFilter = pair.Value;
                        break;
                    default:
                        currentFilterMode = Mode.None;
                        currentFilter = string.Empty;
                        MessageBox.Show("Filter <type> is either Name or Value.", "Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                }

                UpdateListView();
                UpdateStatusBar();
            }
        }

        private void ClearFilter()
        {
            currentFilterMode = Mode.None;
            currentFilter = string.Empty;

            UpdateListView();
            UpdateStatusBar();
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
