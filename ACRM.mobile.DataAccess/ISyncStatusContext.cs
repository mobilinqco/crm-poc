using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.DataAccess
{
    public interface ILocalFileStorageContext
    {
        T GetContent<T>(string fileName);
        Task<T> SaveContent<T>(T content, string fileName);
    }
}
