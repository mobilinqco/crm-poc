using ACRM.mobile.Domain.Application.Questionnaire;
using ACRM.mobile.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Text;
using static ACRM.mobile.UIModels.QuestionnaireEditModel;

namespace ACRM.mobile.ViewModels.ObservableGroups.QuestionnaireEdit
{
    public class BindableQuestionnaireTextAnswer : ExtendedBindableObject
    {
        private bool _isVisible = true;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }

        private string _currentContentValue = "";
        public string CurrentContentValue
        {
            get => _currentContentValue;
            set
            {
                _currentContentValue = value;
                RaisePropertyChanged(() => CurrentContentValue);
            }
        }
        
        private BindableQuestionnaireTextQuestionData _bindableQuestionnaireTextQuestionData;

        public BindableQuestionnaireTextAnswer(BindableQuestionnaireTextQuestionData bindableQuestionnaireTextQuestionData, string contentValue)
        {
            _bindableQuestionnaireTextQuestionData = bindableQuestionnaireTextQuestionData;
            CurrentContentValue = contentValue;
        }

        public bool ApplyViewTypeFilter(QuestionnaireEditViewTypes viewType)
        {
            switch (viewType)
            {
                case QuestionnaireEditViewTypes.Mandatory:
                    IsVisible = _bindableQuestionnaireTextQuestionData.IsMandatory();
                    IsEnabled = true;
                    break;
                case QuestionnaireEditViewTypes.Summary:
                    IsVisible = !string.IsNullOrEmpty(CurrentContentValue);
                    IsEnabled = false;
                    break;
                case QuestionnaireEditViewTypes.All:
                default:
                    IsVisible = true;
                    IsEnabled = true;
                    break;
            }
            return IsVisible;
        }

        public bool IsCompleted()
        {
            return !string.IsNullOrEmpty(CurrentContentValue);
        }
    }
}
