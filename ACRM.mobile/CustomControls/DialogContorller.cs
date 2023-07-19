using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ACRM.mobile.Localization;

namespace ACRM.mobile.CustomControls
{
    public class DialogContorller: IDialogContorller
    {
        protected readonly ILocalizationController _localizationController;
        IProgressDialog progressDialog;

        public DialogContorller(ILocalizationController localizationController)
        {
            _localizationController = localizationController;
        }

        public Task ShowAlertAsync(string message, string title, string buttonLabel)
        {
            HideProgress();
            return UserDialogs.Instance.AlertAsync(message, title, buttonLabel);
        }

        public Task<bool> ShowConfirm(string message, string title, string okLabel, string cancelLabel)
        {
            HideProgress();
            return UserDialogs.Instance.ConfirmAsync(message, title, okLabel, cancelLabel);
        }

        public void ShowProgress(string title)
        {
            var config = new ProgressDialogConfig()
                .SetMaskType(MaskType.Clear)
                .SetTitle(title)
                .SetIsDeterministic(false);

            progressDialog = UserDialogs.Instance.Progress(config);
            progressDialog.Show();
        }

        public void HideProgress()
        {
            if (progressDialog != null && progressDialog.IsShowing)
            {
                progressDialog.Hide();
            }
        }

        public async Task ShowErrorMessageAsync(string textGroup, int titleKey, int messageKey)
        {
            await ShowAlertAsync(_localizationController.GetString(textGroup, messageKey),
                _localizationController.GetString(textGroup, titleKey),
                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
        }

        public async Task ShowErrorMessageAsync(string textGroup, int titleKey, int messageKey, string[] messageParams)
        {
            await ShowAlertAsync(_localizationController.GetFormatedString(textGroup, messageKey, messageParams),
                _localizationController.GetString(textGroup, titleKey),
                _localizationController.GetString(LocalizationKeys.TextGroupBasic, LocalizationKeys.KeyBasicClose));
        }
    }
}
