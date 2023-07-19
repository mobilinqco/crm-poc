using System;
using System.Threading.Tasks;

namespace ACRM.mobile.CustomControls
{
    public interface IDialogContorller
    {
        Task ShowAlertAsync(string message, string title, string buttonLabel);
        Task ShowErrorMessageAsync(string textGroup, int titleKey, int messageKey);
        Task ShowErrorMessageAsync(string textGroup, int titleKey, int messageKey, string[] messageParams);
        Task<bool> ShowConfirm(string message, string title, string okLabel, string cancelLabel);

        void ShowProgress(string message);
        void HideProgress();
    }
}
