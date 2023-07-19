using System;
using System.Threading;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.CustomControls.EditControls.Models
{
    public class TextEditorControlModel : BaseEditControlModel
    {
        public TextEditorControlModel(ListDisplayField field, CancellationTokenSource parentCancellationTokenSource)
            : base(field, parentCancellationTokenSource)
        {
        }
    }
}
