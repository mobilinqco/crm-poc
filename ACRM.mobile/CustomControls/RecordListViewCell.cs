using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.Utils;
using Xamarin.CommunityToolkit.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace ACRM.mobile.CustomControls
{
    public class RecordListViewCell : BaseViewCell
    {
        private double CellWrapperColumnSpacing = 4;
        private Color CellBackgroundColor = Color.White;
        private string MaterialDesignFontFamilly = "MaterialDesign";
        private double ButtonDetailsWidth = 20;
        private double ButtonCategoryWidth = 26;
        private Thickness ButtonDetailsMargins = new Thickness(0, 0, 5, 0);
        private double FontSize = 12;
        private double IconFontSize = 16;
        private Color CellTextColor = Color.DarkGray;
        private Color BackgroundColor = Color.Blue;
        private Thickness CellDataGridPadding = new Thickness(0, 10, 0, 5);
        private double DataRowSpacing = 10;
        private double DataLabelHeight = 20;

        public RecordListViewCell()
        {
            GetResourceValues();
            Grid cellWrapper = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Margin = new Thickness(0, 0, 0, 2),
                BackgroundColor = CellBackgroundColor
            };

            Label label1 = new Label();
            label1.Text = "RecordListView Cell";
            cellWrapper.Children.Add(label1, 0, 0);
            View = cellWrapper;
            View.BindingContextChanged += OnBindingContextChanged;
        }

        protected void OnBindingContextChanged(object sender, EventArgs e)
        {
            if(sender is Grid cellWrapper)
            {
                if (cellWrapper.BindingContext is ListDisplayRow data)
                {
                    GenerateCellContent(cellWrapper, data);
                }
            }
        }

        private void GenerateCellContent(Grid cellWrapper, ListDisplayRow data)
        {
            cellWrapper.Children.Clear();
            cellWrapper.ColumnDefinitions.Clear();
            cellWrapper.RowDefinitions.Clear();

            cellWrapper.ColumnSpacing = CellWrapperColumnSpacing;

            Color categoryColor = ResolveCategoryColor(data);

            BoxView boxView = PrepareInfoAreaColorLine(categoryColor);
            Image categoryImage = PrepareInfoAreaImage(data);
            List<Label> labels = PrepareDataLabels(data);
            Grid contentGrid = PrepareLabelsContentGrid(labels);
            Grid contentRight = PrepareRightGrid(data);

            cellWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            if (categoryImage != null)
            {
                cellWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
            cellWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            if (data.IsRetrievedOnline)
            {
                cellWrapper.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            int colId = 0;
            cellWrapper.Children.Add(boxView, colId++, 0);

            if (categoryImage != null)
            {
                if (data.RowDecorators.LeftMarginColor != null)
                {
                    IconTintColorEffect.SetTintColor(categoryImage, categoryColor);
                }
                cellWrapper.Children.Add(categoryImage, colId++, 0);
            }

            cellWrapper.Children.Add(contentGrid, colId++, 0);

            if (data.IsRetrievedOnline)
            {
                cellWrapper.Children.Add(contentRight, colId, 0);
            }

        }

        private Grid PrepareRightGrid(ListDisplayRow data)
        {
            Grid contentRight = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(0, 0, 0, 0),
                ColumnSpacing = 0,
                RowSpacing = 0,
            };

            contentRight.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            contentRight.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

            if (data.IsRetrievedOnline)
            {
                string pathData = "M 0,0 L 25,0 25,25Z";
                Path path = new Path
                {
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData),
                    Fill = BackgroundColor,
                    Stroke = CellBackgroundColor
                };

                Label lblCloud = new Label
                {
                    FontSize = IconFontSize,
                    TextColor = CellBackgroundColor,
                    Text = MaterialDesignIcons.CloudOutline,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    FontFamily = MaterialDesignFontFamilly
                };

                contentRight.Children.Add(path, 0, 0);
                contentRight.Children.Add(lblCloud, 0, 0);
            }

            BoxView boxView = new BoxView();
            boxView.WidthRequest = 1;
            boxView.VerticalOptions = LayoutOptions.FillAndExpand;
            boxView.BackgroundColor = Color.Transparent;
            contentRight.Children.Add(boxView, 0, 1);

            /* TODO: Enable when implementation of minidetails is done
            Button button = new Button
            {
                FontFamily = MaterialDesignFontFamilly,
                Text = MaterialDesignIcons.DotsVertical,
                TextColor = CellTextColor,
                FontSize = 30,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.Transparent,
                WidthRequest = ButtonDetailsWidth,
                Margin = ButtonDetailsMargins
            };
            contentRight.Children.Add(button, 0, 1);
            */
            return contentRight;
        }

        private Grid PrepareLabelsContentGrid(List<Label> labels)
        {
            Grid contentGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = CellDataGridPadding,
                ColumnSpacing = 4,
                Margin = 0,
                RowSpacing = DataRowSpacing
            };

            
            if (labels.Count < 4)
            {
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                int row = 0;
                labels.ForEach(lbl =>
                {
                    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    contentGrid.Children.Add(lbl, 0, row);
                    row++;
                });
            }
            else if (labels.Count > 0)
            {
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
                int col = 0;
                int row = 0;
                labels.ForEach(lbl =>
                {
                    if (col == 0)
                    {
                        contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    }

                    contentGrid.Children.Add(lbl, col, row);

                    row++;
                    if (row == (labels.Count % 2 + labels.Count / 2))
                    {
                        row = 0;
                        col = 1;
                    }
                });
            }

            return contentGrid;
        }

        private List<Label> PrepareDataLabels(ListDisplayRow data)
        {
            List<Label> labels = new List<Label>();
            int i = 0;
            data.Fields.ForEach(field =>
            {
                if (!field.Config.PresentationFieldAttributes.Hide)
                {
                    Label lbl = new Label
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        FontSize = FontSize,
                        TextColor = CellTextColor,
                        LineBreakMode = LineBreakMode.TailTruncation,
                        HeightRequest = DataLabelHeight
                    };

                    if (i == 0)
                    {
                        lbl.FontAttributes = FontAttributes.Bold;
                    }

                    PrepareLabel(field, lbl);

                    labels.Add(lbl);

                    i++;
                }
            });
            return labels;
        }

        private Image PrepareInfoAreaImage(ListDisplayRow data)
        {
            Image categoryImage = null;

            if (!string.IsNullOrEmpty(data.RowDecorators.Image.image))
            {
                categoryImage = new Image
                {
                    Aspect = Aspect.AspectFit,
                    Source = ImageSource.FromFile(data.RowDecorators.Image.image),
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    WidthRequest = ButtonCategoryWidth
                };
            }
            else if (!string.IsNullOrEmpty(data.RowDecorators.Image.glyph))
            {
                categoryImage = new Image
                {
                    Aspect = Aspect.AspectFit,
                    Source = ImageSource.FromFile(data.RowDecorators.Image.glyph),
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    WidthRequest = ButtonCategoryWidth
                };
            }

            return categoryImage;
        }

        private BoxView PrepareInfoAreaColorLine(Color categoryColor)
        {
            BoxView boxView = new BoxView();
            boxView.WidthRequest = 7;
            boxView.VerticalOptions = LayoutOptions.FillAndExpand;
            boxView.BackgroundColor = categoryColor;
            return boxView;
        }

        private Color ResolveCategoryColor(ListDisplayRow data)
        {
            Color categoryColor;
            if (data.RowDecorators.LeftMarginColor != null)
            {
                categoryColor = Color.FromHex(data.RowDecorators.LeftMarginColor);
            }
            else
            {
                categoryColor = Color.Transparent;
            }

            return categoryColor;
        }

        private void GetResourceValues()
        {
            ResourceDictionary StaticResources = Application.Current.Resources;
            if (StaticResources.ContainsKey("RecordListViewCellTextColor"))
            {
                CellTextColor = (Color)StaticResources["RecordListViewCellTextColor"];
            }

            if (StaticResources.ContainsKey("RecordListViewCellLabelTextSize"))
            {
                FontSize = (OnPlatform<double>)StaticResources["RecordListViewCellLabelTextSize"];
            }

            if (StaticResources.ContainsKey("RecordListViewDetailsButtonMargins"))
            {
                ButtonDetailsMargins = (OnPlatform<Thickness>)StaticResources["RecordListViewDetailsButtonMargins"];
            }

            if (StaticResources.ContainsKey("RecordListViewDetailsButtonWidth"))
            {
                ButtonDetailsWidth = (OnPlatform<double>)StaticResources["RecordListViewDetailsButtonWidth"];
            }

            if (StaticResources.ContainsKey("MaterialDesignIcons"))
            {
                MaterialDesignFontFamilly = (OnPlatform<string>)StaticResources["MaterialDesignIcons"];
            }

            if (StaticResources.ContainsKey("RecordListViewCellBackgroundColor"))
            {
                CellBackgroundColor = (Color)StaticResources["RecordListViewCellBackgroundColor"];
            }

            if (StaticResources.ContainsKey("BackgroundColor"))
            {
                BackgroundColor = (Color)StaticResources["BackgroundColor"];
            }

            if (StaticResources.ContainsKey("RecordListCellDataGridPadding"))
            {
                CellDataGridPadding = (OnPlatform<Thickness>)StaticResources["RecordListCellDataGridPadding"];
            }

            if (StaticResources.ContainsKey("RecordListCellDataRowSpacing"))
            {
                DataRowSpacing = (OnPlatform<double>)StaticResources["RecordListCellDataRowSpacing"];
            }

            if (StaticResources.ContainsKey("RecordListCellDataLabelHeight"))
            {
                DataLabelHeight = (OnPlatform<double>)StaticResources["RecordListCellDataLabelHeight"];
            }
        }
    }
}
