using System;
using System.Collections.Generic;
using esqSimpleUtility.Model;

namespace esqSimpleUtility.Services
{
    public interface IDataService
    {
        List<NameValuePair> GetAll();
        bool Add(NameValuePair item);
        bool Delete(Guid id);
    }
}
