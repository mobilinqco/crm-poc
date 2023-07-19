using ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class QuestionnaireQuestionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate QuestionnaireSelectableQuestionData { get; set; }
        public DataTemplate QuestionnaireTextQuestionData { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is BindableQuestionnaireSelectableQuestionData)
            {
                return QuestionnaireSelectableQuestionData;
            }
            return QuestionnaireTextQuestionData;
        }

    }
}
