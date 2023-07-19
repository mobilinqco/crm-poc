using System;
using System.Threading;
using ACRM.mobile.Domain.Application;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class TextControlModel : BaseEditControlModel
    {
        private Keyboard _keyboardType = Keyboard.Text;
        public Keyboard KeyboardType
        {
            get => _keyboardType;
            set
            {
                _keyboardType = value;
                RaisePropertyChanged(() => KeyboardType);
            }
        }

        public TextControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
            if (field.Config.PresentationFieldAttributes.IsNumeric)
            {
                KeyboardType = Keyboard.Numeric;
            }
            else if (field.Config.PresentationFieldAttributes.Phone)
            {
                KeyboardType = Keyboard.Telephone;
            }
        }

    }
}
