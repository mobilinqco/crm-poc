using System;
using System.Globalization;
using System.Threading;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Localization;
using ACRM.mobile.Logging;
using ACRM.mobile.Utils;
using ACRM.mobile.ViewModels.Base;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.CustomControls
{
    public class UIViewBuilder
    {
        protected Color TitleTextColor = Color.Black;
        protected double TitleFontSize = 16;

        protected Color LabelTextColor = Color.DarkGray;
        protected Color LabelTextEmtpyColor = Color.LightGray;
        protected Color EditLabelTextColor = Color.DarkGray;
        protected double LabelFontSize = 14;

        protected Color DataTextColor = Color.DarkGray;
        protected double DataFontSize = 16;

        protected Thickness CellMargins = new Thickness(0, 20, 0, 0);

        protected readonly INavigationController _navigationController;
        protected readonly ILocalizationController _localizationController;
        protected readonly ILogService _logService;

        public UIViewBuilder(INavigationController navigationController, ILocalizationController localizationController, ILogService logService)
        {
            _navigationController = navigationController;
            _localizationController = localizationController;
            _logService = logService;
            GetResourceValues();

        }

        public void GenerateDetailsContent(Grid cellWrapper, PanelData data)
        {
            if(data.Fields == null || data.Fields.Count == 0)
            {
                return;
            }

            cellWrapper.ClearCellWrapper();
            cellWrapper.Margin = CellMargins;

            StackLayout layout = new StackLayout
            {
                Margin = new Thickness(0, 0, 0, 0),
                Orientation = StackOrientation.Vertical
            };

            Label titleLabel = new Label
            {
                FontSize = TitleFontSize,
                TextColor = TitleTextColor,
                LineBreakMode = LineBreakMode.TailTruncation,
                Margin = new Thickness(10, 10, 0, 0)
            };

            titleLabel.Text = data.Label != null ? data.Label.ToUpper() : "";
            layout.Children.Add(titleLabel);

            BoxView boxView = new BoxView();
            boxView.HeightRequest = 2;
            boxView.HorizontalOptions = LayoutOptions.FillAndExpand;
            boxView.BackgroundColor = Color.Black;
            layout.Children.Add(boxView);

            Grid contentGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(10, 10, 0, 0)
            };

            if (data.Type == PanelType.Grid)
            {
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            int row = 0;
            int col = 0;

            data.Fields.ForEach(field =>
            {
                if (!field.Config.PresentationFieldAttributes.Hide && !field.Config.PresentationFieldAttributes.Empty)
                {
                    Grid fieldGrid = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };

                    int fieldGridRow = 0;

                    if (!field.Config.PresentationFieldAttributes.NoLabel)
                    {
                        fieldGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        Label fieldLabel = new Label
                        {
                            FontSize = LabelFontSize,
                            TextColor = field.Data.IsNullOrEmpty() ? LabelTextEmtpyColor : LabelTextColor,
                            LineBreakMode = LineBreakMode.TailTruncation,
                            Text = field.Config.PresentationFieldAttributes.Label()
                        };
                        fieldGrid.Children.Add(fieldLabel, 0, fieldGridRow++);
                    }

                    fieldGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    fieldGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    Label fieldData = new Label
                    {
                        FontSize = DataFontSize,
                        TextColor = DataTextColor,
                        MaxLines = 4,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        VerticalOptions = LayoutOptions.Center
                    };

                    PrepareLabel(field, fieldData);

                    if (field.HasLinkButton())
                    {
                        Grid labelWithButtonGrid = new Grid
                        {
                            HorizontalOptions = LayoutOptions.Fill,
                            VerticalOptions = LayoutOptions.Fill,
                            Padding = new Thickness(0, 0, 10, 0),
                        };
                        labelWithButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        labelWithButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

                        var linkButton = PrepareLinkButton(field);

                        labelWithButtonGrid.Children.Add(fieldData, 0, 0);
                        labelWithButtonGrid.Children.Add(linkButton, 1, 0);
                        fieldGrid.Children.Add(labelWithButtonGrid, 0, fieldGridRow++);
                    }
                    else
                    {
                        fieldGrid.Children.Add(fieldData, 0, fieldGridRow++);
                    }

                    BoxView fieldSeparator = new BoxView();
                    fieldSeparator.HeightRequest = 1;
                    fieldSeparator.HorizontalOptions = LayoutOptions.Fill;
                    fieldSeparator.VerticalOptions = LayoutOptions.End;
                    fieldSeparator.BackgroundColor = Color.LightGray;
                    fieldGrid.Children.Add(fieldSeparator, 0, fieldGridRow++);

                    if (col == 0)
                    {
                        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    AddFieldGrid(data, contentGrid, ref row, ref col, fieldGrid);
                }
                else if (field.Config.PresentationFieldAttributes.Empty)
                {
                    Grid fieldGrid = new Grid
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                    BoxView fieldSeparator = FieldSeparator();
                    fieldGrid.Children.Add(fieldSeparator, 0, 0);
                    AddFieldGrid(data, contentGrid, ref row, ref col, fieldGrid);
                }
            });

            layout.Children.Add(contentGrid);
            cellWrapper.Children.Add(layout);
        }

        private void AddFieldGrid(PanelData data, Grid contentGrid, ref int row, ref int col, Grid fieldGrid)
        {
            contentGrid.Children.Add(fieldGrid, col, row);

            if (data.Type == PanelType.Grid)
            {
                if (col == 1)
                {
                    col = 0;
                    row++;
                }
                else
                {
                    col = 1;
                }
            }
            else
            {
                row++;
            }
        }

        private BoxView FieldSeparator()
        {
            BoxView fieldSeparator = new BoxView();
            fieldSeparator.HeightRequest = 1;
            fieldSeparator.HorizontalOptions = LayoutOptions.Fill;
            fieldSeparator.VerticalOptions = LayoutOptions.End;
            fieldSeparator.BackgroundColor = Color.LightGray;
            return fieldSeparator;
        }

        public void PrepareLabel( ListDisplayField field, Label lbl)
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
            if (!String.IsNullOrEmpty(field.Config.PresentationFieldAttributes.Color))
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

        public ImageButton PrepareLinkButton(ListDisplayField field)
        {
            UserAction userAction = null;
            string value = _localizationController.GetLocalizedValue(field);
            var linkIcon = MaterialDesignIcons.ArrowRightBoldCircle;
            bool userLauncher = true;
            bool isActive = !string.IsNullOrWhiteSpace(value);

            if (field.Config.PresentationFieldAttributes.Email)
            {
                linkIcon = MaterialDesignIcons.Email;
                if (isActive)
                {
                    if (!value.Contains("mailto"))
                    {
                        value = $"mailto:{value}";
                    }
                }
            }
            else if (field.Config.PresentationFieldAttributes.Phone)
            {
                linkIcon = MaterialDesignIcons.Phone;
                if (isActive)
                {
                    if (!value.Contains("tel"))
                    {
                        value = $"tel:{value}";
                    }
                }
            }
            else if (field.Config.PresentationFieldAttributes.Hyperlink)
            {
                userLauncher = false;
                linkIcon = MaterialDesignIcons.ArrowRightBoldCircle;

                if (field.Data.LinkedFieldData != null)
                {
                    if (!field.Data.LinkedFieldData.IsIncomplete)
                    {
                        userAction = new UserAction
                        {
                            RecordId = field.Data.LinkedFieldData.RecordId,
                            InfoAreaUnitName = field.Data.LinkedFieldData.InfoAreaId,
                            ActionUnitName = "SHOWRECORD",
                            ActionType = UserActionType.ShowRecord
                        };
                    }
                    else
                    {
                        isActive = false;
                    }
                }
                else
                {
                    if (isActive)
                    {
                        if (!value.Contains("http"))
                        {
                            value = $"https://{value}";
                        }
                    }
                }
            }

            return CreateLinkButton(field, value, linkIcon, userLauncher, userAction, isActive);
        }

        private ImageButton CreateLinkButton(ListDisplayField field, string value, string linkIcon, bool UserLauncher, UserAction userAction = null, bool isActive = true)
        {
            ImageButton linkButton = new ImageButton
            {
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.Transparent
            };
            ResourceDictionary StaticResources = Application.Current.Resources;

            linkButton.Source = new FontImageSource
            {
                FontFamily = (OnPlatform<string>)StaticResources["MaterialDesignIcons"],
                Glyph = linkIcon,
                Color = isActive ? (Color)StaticResources["LinkButtonColor"] : (Color)StaticResources["LinkEmptyButtonColor"],
                Size = (OnPlatform<double>)StaticResources["LinkIconFontSize"]
            };

            if (isActive)
            {
                linkButton.Clicked += async (s, e) =>
                {
                    try
                    {
                        if (UserLauncher)
                        {
                            await Launcher.OpenAsync(new Uri(value));
                        }
                        else
                        {
                            if (userAction != null)
                            {
                                var navigationController = AppContainer.Resolve<INavigationController>();
                                await navigationController.SimpleNavigateWithAction(userAction);
                            }
                            else
                            {
                                await Browser.OpenAsync(new Uri(value), BrowserLaunchMode.SystemPreferred);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        _logService.LogError($"{"Unable to format Url for field value: " + value + ". Error: " + ex.GetType().Name + " : " + ex.Message}");
                    }
                };
            }

            return linkButton;
        }

        private void GetResourceValues()
        {
            ResourceDictionary StaticResources = Application.Current.Resources;
            if (StaticResources.ContainsKey("PanelCellTitleTextColor"))
            {
                TitleTextColor = (Color)StaticResources["PanelCellTitleTextColor"];
            }

            if (StaticResources.ContainsKey("PanelCellTitleTextSize"))
            {
                TitleFontSize = (OnPlatform<double>)StaticResources["PanelCellTitleTextSize"];
            }

            if (StaticResources.ContainsKey("PanelCellLabelTextColor"))
            {
                LabelTextColor = (Color)StaticResources["PanelCellLabelTextColor"];
            }

            if (StaticResources.ContainsKey("PanelCellLabelTextEmtpyColor"))
            {
                LabelTextEmtpyColor = (Color)StaticResources["PanelCellLabelTextEmtpyColor"];
            }


            if (StaticResources.ContainsKey("PanelCellEditLabelTextColor"))
            {
                EditLabelTextColor = (Color)StaticResources["PanelCellEditLabelTextColor"];
            }

            if (StaticResources.ContainsKey("PanelCellLabelFontSize"))
            {
                LabelFontSize = (OnPlatform<double>)StaticResources["PanelCellLabelFontSize"];
            }

            if (StaticResources.ContainsKey("PanelCellDataTextColor"))
            {
                DataTextColor = (Color)StaticResources["PanelCellDataTextColor"];
            }

            if (StaticResources.ContainsKey("PanelCellDataFontSize"))
            {
                DataFontSize = (OnPlatform<double>)StaticResources["PanelCellDataFontSize"];
            }

            if (StaticResources.ContainsKey("PanelCellMargins"))
            {
                CellMargins = (OnPlatform<Thickness>)StaticResources["PanelCellMargins"];
            }
        }
    }
}
