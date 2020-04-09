using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using esqSimpleUtility.Model;
using esqSimpleUtility.Services;

namespace esqSimpleUtility.Tests
{
    [TestClass]
    public class DataServiceTest
    {
        [TestMethod]
        public void BasicOperations()
        {
            var service = new SimpleDataService();
            var data = service.GetAll();
            int count = data.Count;
            bool result;
            NameValuePair pair;

            var testPair = new NameValuePair
            {
                Id = Guid.NewGuid(),
                Name = "TestName",
                Value = "TestValue",
            };

            // Add
            result = service.Add(testPair);
            Assert.IsTrue(result);

            data = service.GetAll();
            Assert.IsTrue(count + 1 == data.Count);

            pair = data.Find(x => x?.Id == testPair.Id);
            Assert.IsNotNull(pair);
            Assert.IsTrue(pair.Equals(testPair));

            // Delete
            result = service.Delete(pair.Id);
            Assert.IsTrue(result);

            data = service.GetAll();
            Assert.IsTrue(count == data.Count);

            pair = data.Find(x => x?.Id == testPair.Id);
            Assert.IsNull(pair);
        }
    }
}
