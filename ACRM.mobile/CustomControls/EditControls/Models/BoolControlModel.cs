using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class BoolControlModel: BaseEditControlModel
    {
        public BoolControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {

        }

        protected override void InitaizeEdit(ListDisplayField field)
        {
            AllowedValues.Add(new SelectableFieldValue
            {
                RecordId = "true",
                DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                    LocalizationKeys.KeyBasicYes)
            });
            AllowedValues.Add(new SelectableFieldValue
            {
                RecordId = "false",
                DisplayValue = _localizationController.GetString(LocalizationKeys.TextGroupBasic,
                    LocalizationKeys.KeyBasicNo)
            });

            if (field.EditData.DefaultSelectedValue != null
                && (field.EditData.DefaultSelectedValue.RecordId.Equals("1")
                || field.EditData.DefaultSelectedValue.RecordId.ToLower().Equals("true")))
            {
                field.EditData.DefaultSelectedValue = AllowedValues[0];
            }
            else
            {
                field.EditData.DefaultSelectedValue = AllowedValues[1];
            }

            base.InitaizeEdit(field);
        }
    }
}
