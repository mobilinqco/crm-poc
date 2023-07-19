using System.Globalization;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Localization;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class CalendarEventDetailsPanelViewBuilder
    {
        protected readonly ILocalizationController _localizationController;

        protected Color DataTextColor = Color.Black;
        protected double DataFontSize = 16;

        protected Color LabelTextColor = Color.DarkGray;
        protected Color LabelTextEmtpyColor = Color.LightGray;
        protected double LabelFontSize = 16;

        protected double SeparatorHeight = 1;
        protected Color SeparatorColor = Color.DarkGray;

        protected double ColumnSpacing = 5;
        protected double BottomGridColumnSpacing = 20;
        protected double RowSpacing = 5;

        public CalendarEventDetailsPanelViewBuilder(ILocalizationController localizationController)
        {
            _localizationController = localizationController;
        }

        public void GenerateDetailsContent(Grid cellWrapper, PanelData data)
        {
            cellWrapper.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            cellWrapper.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            cellWrapper.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Grid topGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                ColumnSpacing = ColumnSpacing,
                RowSpacing = RowSpacing
            };
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            BoxView separator = new BoxView
            {
                HeightRequest = SeparatorHeight,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start,
                BackgroundColor = SeparatorColor,
                Margin = 0
            };

            Grid bottomGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                ColumnSpacing = BottomGridColumnSpacing,
                RowSpacing = RowSpacing
            };
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            int topGridRowIndex = 0;
            int bottomGridRowIndex = 0;

            for (int i = 0; i<data.Fields.Count; i++)
            {
                // First four fields are on the top part without label
                if (i < 4)
                {
                    int columnIndex = (i % 2 == 0) ? 0 : 1;

                    if (columnIndex == 0)
                    {
                        topGrid.Children.Add(GenerateTopGridDataTextLabel(LayoutOptions.Start, data.Fields[i]), columnIndex, topGridRowIndex);
                    }
                    else
                    {
                        topGrid.Children.Add(GenerateTopGridDataTextLabel(LayoutOptions.End, data.Fields[i]), columnIndex, topGridRowIndex);
                        topGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                        topGridRowIndex++; // Each row, in the case of the first 4 fields, contains 2 labels
                    }
                }
                else
                {
                    bottomGrid.Children.Add(GenerateBottomGridLabelTextLabel(data.Fields[i]), 0, bottomGridRowIndex);
                    bottomGrid.Children.Add(GenerateBottomGridDataTextLabel(data.Fields[i]), 1, bottomGridRowIndex);
                    bottomGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    bottomGridRowIndex++;
                }
            }

            cellWrapper.Children.Add(topGrid, 0, 0);
            cellWrapper.Children.Add(separator, 0, 1);
            cellWrapper.Children.Add(bottomGrid, 0, 2);
        }

        private Label GenerateTopGridDataTextLabel(LayoutOptions horizontalOptions, ListDisplayField field)
        {
            var fieldData =  new Label
            {
                HorizontalOptions = horizontalOptions,
                VerticalOptions = LayoutOptions.Center,
                FontSize = DataFontSize,
                TextColor = DataTextColor
            };

            PrepareLabel(field, fieldData);

            return fieldData;
        }

        private Label GenerateBottomGridLabelTextLabel(ListDisplayField field)
        {
            var fieldLabel = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = LabelFontSize,
                TextColor = LabelTextColor,
                Text = field.Config.PresentationFieldAttributes.Label()
            };

            return fieldLabel;
        }

        private Label GenerateBottomGridDataTextLabel(ListDisplayField field)
        {
            var fieldData = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontSize = DataFontSize,
                TextColor = DataTextColor
            };

            PrepareLabel(field, fieldData);

            return fieldData;
        }

        public void PrepareLabel(ListDisplayField field, Label lbl)
        {
            // Bold and/or Italic
            if (field.Config.PresentationFieldAttributes.Bold && field.Config.PresentationFieldAttributes.Italic)
            {
                lbl.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;
            }
            else if (field.Config.PresentationFieldAttributes.Italic)
            {
                lbl.FontAttributes = FontAttributes.Italic;
            }
            else if (field.Config.PresentationFieldAttributes.Bold)
            {
                lbl.FontAttributes = FontAttributes.Bold;
            }

            // Label color
            if (!string.IsNullOrEmpty(field.Config.PresentationFieldAttributes.Color))
            {
                var convertor = new StringToColorConverter();
                lbl.TextColor = (Color)convertor.Convert(field.Config.PresentationFieldAttributes.Color, null, null, CultureInfo.CurrentCulture);
            }

            string fieldData = _localizationController.GetLocalizedValue(field);

            if (string.IsNullOrEmpty(fieldData))
            {
                fieldData = " ";
            }

            lbl.Text = fieldData;
        }
    }
}
