using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using esqSimpleUtility.Model;
using esqSimpleUtility.Services;
using esqSimpleUtility.ViewModel;
using System.Collections.Generic;

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
        }
    }
}
