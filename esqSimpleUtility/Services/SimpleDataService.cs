using System;
using System.Collections.Generic;
using esqSimpleUtility.Model;

namespace esqSimpleUtility.Services
{
    /// <summary>
    /// This is a very basic data service for the test task.
    /// </summary>
    public class SimpleDataService : IDataService
    {
        private List<NameValuePair> data;

        public SimpleDataService()
        {
            data = new List<NameValuePair>
            {
                new NameValuePair { Id = Guid.NewGuid(), Name = "Hello", Value= "World", PaddingLeft = 1, PaddingRight = 1 },
                new NameValuePair { Id = Guid.NewGuid(), Name = "FirstName", Value= "Bob", PaddingLeft = 1, PaddingRight = 1 },
                new NameValuePair { Id = Guid.NewGuid(), Name = "Language", Value= "CSharp", PaddingLeft = 1, PaddingRight = 1 },
            };
        }

        public List<NameValuePair> GetAll()
        {
            var result = new List<NameValuePair>(data.Count);
            data.ForEach((item) =>
            {
                result.Add(NameValuePair.CopyFrom(item));
            });

            return result;
        }

        public bool Add(NameValuePair item)
        {
            if (item == null)
                return false;

            if (data.Find(x => x.Id == item.Id) != null)
                return false;

            data.Add(NameValuePair.CopyFrom(item));
            return true;
        }

        public bool Delete(Guid id)
        {
            int n = data.RemoveAll(x => x.Id == id);
            return (n > 0) ? true : false;
        }
    }
}
