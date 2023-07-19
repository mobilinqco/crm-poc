using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ACRM.mobile.Domain.Application;
using ACRM.mobile.ViewModels.Base;
using SkiaSharp;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ACRM.mobile.ViewModels.ObservableGroups
{
    public class HeaderActionButton : ExtendedBindableObject
    {
        public UserAction UserAction { get; set; }

        private string _userActionImagePath;
        public string UserActionImagePath
        {
            get => _userActionImagePath;
            set
            {
                _userActionImagePath = value;
                RaisePropertyChanged(() => UserActionImagePath);
            }
        }

        private string _userActionDisplayName;
        public string UserActionDisplayName
        {
            get => _userActionDisplayName;
            set
            {
                _userActionDisplayName = value;
                RaisePropertyChanged(() => UserActionDisplayName);
            }
        }

        public HeaderActionButton(UserAction userAction)
        {
            UserAction = userAction;
            GenerateHeaderActionButtonImage();
            UserActionDisplayName = userAction.ActionDisplayName;
        }

        private void GenerateHeaderActionButtonImage()
        {
            if (UserAction.DisplayGlyphImageText != null)
            {
                UserActionImagePath = GenerateActionGlyphImage(UserAction.DisplayGlyphImageText);
            }
            else if (UserAction.DisplayImageName != null)
            {
                UserActionImagePath = GenerateActionImage(UserAction);
            }
            else
            {
                // TODO Default Image
            }
        }

        private string GenerateActionGlyphImage(string displayGlyphImageText)
        {
            var bytes = Encoding.UTF32.GetBytes(displayGlyphImageText);
            string glyphTextString = string.Join("", bytes.Select(x => x.ToString("x")));
            string imagePath = Path.Combine(FileSystem.CacheDirectory, $"glyph_action_icon_{glyphTextString}.png");

            if (!File.Exists(imagePath))
            {
                var info = new SKImageInfo(100, 100);

                var surface = SKSurface.Create(info);

                SKCanvas canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);

                SKPaint circlePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.05f * info.Width,
                    Color = SKColors.White,
                    IsAntialias = true
                };

                // Adjust Circle such that the radius is 90% of relative area width
                SKRect circleRect = new SKRect(0.05f * info.Width, 0.05f * info.Height,
                    0.9f * info.Width + 0.05f * info.Width, 0.9f * info.Height + 0.05f * info.Height);

                float xCircle = info.Width / 2;
                float yCircle = info.Height / 2;

                canvas.DrawCircle(xCircle, yCircle, circleRect.Height / 2, circlePaint);

                SKBitmap bitmap = SKBitmap.Decode(DrawGlyphImage(displayGlyphImageText));

                SKPaint bitmapPaint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                };

                // Adjust Bitmap such that the max length is circle radius / sqrt(2) to avoid overlapping
                float squareLength = circleRect.Height / (float)Math.Sqrt(2);
                SKRect bitmapBounds = new SKRect((info.Width - squareLength) / 2, (info.Height - squareLength) / 2,
                    squareLength + (info.Width - squareLength) / 2, squareLength + (info.Width - squareLength) / 2);

                canvas.DrawBitmap(bitmap, bitmapBounds, bitmapPaint);

                SKData encodedData = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);

                using (FileStream bitmapImageStream = File.Open(imagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    encodedData.SaveTo(bitmapImageStream);
                }

                surface.Dispose();
                bitmapPaint.Dispose();
                circlePaint.Dispose();
                encodedData.Dispose();
            }

            return imagePath;
        }

        private string DrawGlyphImage(string displayGlyphImageText)
        {
            var bytes = Encoding.UTF32.GetBytes(displayGlyphImageText);
            string glyphTextString = string.Join("", bytes.Select(x => x.ToString("x")));
            string glyphImagePath = Path.Combine(FileSystem.CacheDirectory, $"glyph_{glyphTextString}.png");

            if (!File.Exists(glyphImagePath))
            {
                var info = new SKImageInfo(100, 100);

                var surface = SKSurface.Create(info);

                SKCanvas canvas = surface.Canvas;

                canvas.Clear(SKColors.Transparent);

                var textPaint = new SKPaint
                {
                    Typeface = GetPlatformTypeface(),
                    Color = new SKColor(255, 255, 255, 255),
                    IsAntialias = true
                };

                // Adjust TextSize property so text is 90% of screen width
                float textWidth = textPaint.MeasureText(displayGlyphImageText);
                textPaint.TextSize = 0.9f * info.Width * textPaint.TextSize / textWidth;

                // TODO iterative method to find TextSize which has desired bounds (better precision)
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(displayGlyphImageText, ref textBounds);

                // Calculate offsets to center the text on the screen
                float xText = info.Width / 2 - textBounds.MidX;
                float yText = info.Height / 2 - textBounds.MidY;

                canvas.DrawText(displayGlyphImageText, xText, yText, textPaint);

                SKData encodedData = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);

                using (FileStream bitmapImageStream = File.Open(glyphImagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    encodedData.SaveTo(bitmapImageStream);
                }

                surface.Dispose();
                textPaint.Dispose();
                encodedData.Dispose();
            }

            return glyphImagePath;
        }

        private SKTypeface GetPlatformTypeface()
        {
            SKTypeface skTypeface = null;

            string platformFontResource = (OnPlatform<string>)Application.Current.Resources["HeaderButtonImagesFont"];

            if (platformFontResource.Contains('.'))
            {
                platformFontResource = platformFontResource.Split('.')[0];
            }

            string devCrmInstancesResourceName = $"ACRM.mobile.Resources.Fonts.{platformFontResource}.ttf";
            using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(devCrmInstancesResourceName))
            {
                skTypeface = SKTypeface.FromStream(resource);
            }

            return skTypeface;
        }

        private string GenerateActionImage(UserAction userAction)
        {
            string imagePath = Path.Combine(FileSystem.CacheDirectory, userAction.IconFileName());

            if(!File.Exists(imagePath))
            {
                var info = new SKImageInfo(100, 100);
                var surface = SKSurface.Create(info);

                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                SKPaint circlePaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.05f * info.Width,
                    Color = SKColors.White,
                    IsAntialias = true
                };

                // Adjust Circle such that the radius is 90% of relative area width
                SKRect circleRect = new SKRect(0.05f * info.Width, 0.05f * info.Height,
                    0.9f * info.Width + 0.05f * info.Width, 0.9f * info.Height + 0.05f * info.Height);

                float xCircle = info.Width / 2;
                float yCircle = info.Height / 2;

                canvas.DrawCircle(xCircle, yCircle, circleRect.Height / 2, circlePaint);

                SKBitmap bitmap = null;
                if (File.Exists(userAction.DisplayImageName))
                {
                    bitmap = SKBitmap.Decode(userAction.DisplayImageName);
                }

                SKPaint bitmapPaint = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.High,
                };

                // Adjust Bitmap such that the max length is circle radius / sqrt(2) to avoid overlapping
                float squareLength = circleRect.Height / (float)Math.Sqrt(2);
                SKRect bitmapBounds = new SKRect((info.Width - squareLength) / 2,
                    (info.Height - squareLength) / 2,
                    squareLength + (info.Width - squareLength) / 2,
                    squareLength + (info.Width - squareLength) / 2);

                if (bitmap != null)
                {
                    canvas.DrawBitmap(bitmap, bitmapBounds, bitmapPaint);
                }

                SKData encodedData = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);

                using (FileStream bitmapImageStream = File.Open(imagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    encodedData.SaveTo(bitmapImageStream);
                }

                surface.Dispose();
                bitmapPaint.Dispose();
                circlePaint.Dispose();
                encodedData.Dispose();
            }

            return imagePath;
        }

        public void UpdateUserActionImage(string imageName, string glyphText)
        {
            UserAction.DisplayImageName = imageName;
            UserAction.DisplayGlyphImageText = glyphText;
            GenerateHeaderActionButtonImage();
        }

        public void UpdateUserActionDisplayName(string displayName)
        {
            UserAction.ActionDisplayName = displayName;
            UserActionDisplayName = displayName;
        }
    }
}
