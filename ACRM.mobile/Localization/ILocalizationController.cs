using System;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.Localization
{
    public interface ILocalizationController
    {
        void AttachConfiguration();
        void ResetConfiguration();
        string GetString(string textGroupKey, int textIndex, string defaultString = "<Not localized>");
        string GetString(string textGroupKey, int textIndex);
        string GetFormatedString(string textGroupKey, int textIndex, params string[] values);
        string GetBoolString(bool isTrue);

        string GetLocalizedValue(ListDisplayField ldf);
    }
}
