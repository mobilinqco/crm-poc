using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.DataModel;

namespace ACRM.mobile.Services.Contracts
{
    public interface IRuleProcessor
    {
        public bool IsExpandRuleTrue(ExpandRule rule, FieldInfo fieldInfo, string value);
    }
}
