using ACRM.mobile.DataAccess;
using ACRM.mobile.DataAccess.Local;
using ACRM.mobile.Domain.Configuration;
using ACRM.mobile.Domain.Configuration.UserInterface;
using System;
using Xunit;

namespace LocalDataAccess.Tests.Configuration
{
    public class AnalysisDataAccessTest
    {
        [Fact]
        public void WebConfigValuesTablesCreated()
        {
            var sessionContext = new SessionContext("");

            var configContext = new ConfigurationContext(sessionContext);
            configContext.Database.EnsureCreated();
            WebConfigValue val = new WebConfigValue();
            val.UnitName = "ABC";
            val.Value = "BCD";
            val.Inherited = 1;
            configContext.Add(val);
            configContext.SaveChanges();

            //configContext.Database.Query(typeof(int),
            //    "select count(*) from sqlite_master where type ='table' and name='Analyses'");
        }

        [Fact]
        public void ConfigurationUnitOfWork()
        {
            var sessionContext = new SessionContext("");
            var configContext = new ConfigurationContext(sessionContext);
            var unitOfWork = new ConfigurationUnitOfWork(configContext);

            //configContext.Database.EnsureCreated();
            WebConfigValue val = new WebConfigValue();
            val.UnitName = "ABC";
            val.Value = "BCD";
            val.Inherited = 1;
            unitOfWork.GenericRepository<WebConfigValue>().Add(val);
            unitOfWork.Save();
            
            //configContext.Database.Query(typeof(int),
            //    "select count(*) from sqlite_master where type ='table' and name='Analyses'");
        }
    }
}
