using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Utils;

namespace ACRM.mobile.UIModels
{
    public class FormRecordListModel : ChildRecordsModel
    {
        public FormItemData formItemData { get; set; }

        public async override ValueTask<bool> InitializeControl()
        {
            await base.InitializeControl();
            await LoadRecords();
            return true;
        }

        private async Task LoadRecords()
        {
            LineHeight = 150;
            if (formItemData?.FormItem != null)
            {
                Title = formItemData.FormItem.Label.ToUpperInvariant();
            }
            if (formItemData?.FormItem != null)
            {
                var prepareDataTask = Task.Run(() =>
                {
                    return _contentService.PrepareRecordsAsync(formItemData, _cancellationTokenSource.Token).Result;

                });
                await prepareDataTask.ContinueWith(
                     antecedent =>
                     {
                         Records = antecedent.Result;
                         SetUIHeight(Records.Count);
                         if (Records.Count == 0)
                         {
                             EnableNoResultsText = true;
                             NoResultsText = _localizationController.GetString(LocalizationKeys.TextGroupErrors,
                                 LocalizationKeys.KeyErrorsNoResults);
                         }
                         else
                         {
                             EnableNoResultsText = false;
                         }
                         IsLoading = false;
                     }, TaskContinuationOptions.OnlyOnRanToCompletion);

            }
        }

        public FormRecordListModel(object widgetArgs, CancellationTokenSource parentCancellationTokenSource)
            : base(widgetArgs, parentCancellationTokenSource)
        {
            if (widgetArgs is FormItemData formData)
            {
                formItemData = formData;
                RegisterMessageIfNotExist(WidgetEventType.FormParamsChanged, "*", OnFormItemChanged);
            }
        }

        private async Task OnFormItemChanged(WidgetMessage arg)
        {
            if (formItemData != null && arg.Data is Dictionary<string, Dictionary<string, string>> Params)
            {
                IsLoading = true;
                formItemData.FormParams = Params;
                await LoadRecords();
            }
        }
    }
}
