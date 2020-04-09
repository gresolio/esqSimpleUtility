using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using esqSimpleUtility.Model;
using esqSimpleUtility.Services;
using esqSimpleUtility.ViewModel;

namespace esqSimpleUtility.Tests
{
    [TestClass]
    public class ViewModelTest
    {
        [TestMethod]
        public void BasicOperations()
        {
            var service = new SimpleDataService();
            var initialData = service.GetAll();
            var target = new SimpleUtilityViewModel(service);
            ICollectionView listView = CollectionViewSource.GetDefaultView(target.ListViewItems);

            // Check ListViewItems before running commands
            bool result = initialData.SequenceEqual(target.ListViewItems);
            Assert.IsTrue(result);

            // Add items
            target.AddCommand.Execute("test1 = value1");
            var item1 = target.ListViewItems.Last();
            Assert.IsTrue(item1.Name == "test1" && item1.Value == "value1");

            target.AddCommand.Execute("test2 = value2");
            var item2 = target.ListViewItems.Last();
            Assert.IsTrue(item2.Name == "test2" && item2.Value == "value2");

            // Delete items
            var selected = new List<object> { item1, item2 };
            target.DeleteCommand.Execute(selected);

            // Check ListViewItems after running commands
            result = initialData.SequenceEqual(target.ListViewItems);
            Assert.IsTrue(result);

            // Sorting
            target.SortCommand.Execute(SimpleUtilityViewModel.Mode.ByName.ToString());
            Assert.IsTrue(listView.SortDescriptions.Count == 1);
            SortDescription desc = listView.SortDescriptions.FirstOrDefault();
            Assert.IsTrue(desc.PropertyName == "Name");
            Assert.IsTrue(desc.Direction == ListSortDirection.Ascending);

            target.SortCommand.Execute(SimpleUtilityViewModel.Mode.ByValue.ToString());
            Assert.IsTrue(listView.SortDescriptions.Count == 1);
            desc = listView.SortDescriptions.FirstOrDefault();
            Assert.IsTrue(desc.PropertyName == "Value");
            Assert.IsTrue(desc.Direction == ListSortDirection.Ascending);

            var filterTestPair = new NameValuePair
            {
                Id = Guid.NewGuid(),
                Name = "FilterTestName",
                Value = "FilterTestValue",
            };

            // Filtering
            target.ApplyFilterCommand.Execute("Name = FilterTestName");
            Assert.IsNotNull(listView.Filter);
            Assert.IsTrue(listView.Filter(filterTestPair));

            target.ApplyFilterCommand.Execute("Value = FilterTestValue");
            Assert.IsNotNull(listView.Filter);
            Assert.IsTrue(listView.Filter(filterTestPair));

            target.ClearFilterCommand.Execute(null);
            Assert.IsNull(listView.Filter);

            target.ApplyFilterCommand.Execute("WrongFilterType = Value");
            Assert.IsNull(listView.Filter);
        }
    }
}
