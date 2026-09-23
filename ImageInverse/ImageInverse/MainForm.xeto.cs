using System;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace ImageInverse
{
    public class MainForm : Form
    {
        private readonly Size maxImageSize = new Size(380, 380);
        private Bitmap selectedBitmap;
        private NumericStepper firstXInput;
        private NumericStepper firstYInput;
        private NumericStepper secondXInput;
        private NumericStepper secondYInput;

        public MainForm()
        {
            XamlReader.Load(this);
            Title = "Image Inverse";
            ClientSize = new Size(820, 640);

            ImageView imageOriginal = new ImageView { Size = maxImageSize };
            ImageView imageResult = new ImageView { Size = maxImageSize };


            Button selectImageButton = new Button
            {
                Text = "Select",
                Command = new Command((s, e) => SelectImage(imageOriginal, imageResult)),
                Size = new Size(120, 32)
            };

            Button invertButton = new Button
            {
                Text = "Invert",
                Command = new Command((s, e) =>
                {
                    if (selectedBitmap != null)
                    {
                        var inverted = Invert(
                            selectedBitmap,
                            (int)firstXInput.Value,
                            (int)firstYInput.Value,
                            (int)secondXInput.Value,
                            (int)secondYInput.Value);
                        imageResult.Image = ScaleToFit(inverted, maxImageSize.Width, maxImageSize.Height);
                    }
                }),
                Size = new Size(120, 32)
            };

            var topBar = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Items =
                {
                    selectImageButton,
                    invertButton
                }
            };

            firstXInput = CreateCoordinateInput();
            firstYInput = CreateCoordinateInput();
            secondXInput = CreateCoordinateInput();
            secondYInput = CreateCoordinateInput();

            var firstPointRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Items =
                {
                    new Label { Text = "First point", VerticalAlignment = VerticalAlignment.Center, Width = 100 },
                    new Label { Text = "X", VerticalAlignment = VerticalAlignment.Center },
                    firstXInput,
                    new Label { Text = "Y", VerticalAlignment = VerticalAlignment.Center },
                    firstYInput
                }
            };

            var secondPointRow = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Items =
                {
                    new Label { Text = "Second point", VerticalAlignment = VerticalAlignment.Center, Width = 100 },
                    new Label { Text = "X", VerticalAlignment = VerticalAlignment.Center },
                    secondXInput,
                    new Label { Text = "Y", VerticalAlignment = VerticalAlignment.Center },
                    secondYInput
                }
            };

            var imagesTable = new TableLayout
            {
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Original" }, true),
                        new TableCell(new Label { Text = "Result" }, true)
                    ),
                    new TableRow(
                        new TableCell(new Scrollable { Content = imageOriginal }, true),
                        new TableCell(new Scrollable { Content = imageResult }, true)
                    )
                }
            };

            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Items = { topBar, firstPointRow, secondPointRow, imagesTable }
            };
        }

        private static NumericStepper CreateCoordinateInput()
        {
            return new NumericStepper
            {
                MinValue = 0,
                MaxValue = 1,
                Value = 0,
                Increment = 1,
                DecimalPlaces = 0,
                Width = 100
            };
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            new AboutDialog() {
				Version = "1.2.0",
				Developers = new[] { "Dinar Dusov MO-401B" },
				ProgramDescription = "Program for Inversing images. Powered by C# and Eto."
			}.ShowDialog(this);
        }

        protected void HandleQuit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        private void SelectImage(ImageView imageOriginal, ImageView imageResult)
        {
            var dlg = new OpenFileDialog
            {
                Filters = { new FileFilter("Images", "*.png", "*.jpg", "*.bmp") }
            };

            if (dlg.ShowDialog(this) == DialogResult.Ok)
            {
                selectedBitmap = new Bitmap(dlg.FileName);
                SetCoordinateBounds(firstXInput, selectedBitmap.Width - 1, 0);
                SetCoordinateBounds(firstYInput, selectedBitmap.Height - 1, 0);
                SetCoordinateBounds(secondXInput, selectedBitmap.Width - 1, selectedBitmap.Width - 1);
                SetCoordinateBounds(secondYInput, selectedBitmap.Height - 1, selectedBitmap.Height - 1);
                imageOriginal.Image = ScaleToFit(selectedBitmap, maxImageSize.Width, maxImageSize.Height);
                imageResult.Image = null;
            }
        }

        private static void SetCoordinateBounds(NumericStepper input, int maximum, int value)
        {
            input.MaxValue = Math.Max(0, maximum);
            input.Value = Math.Min(Math.Max(0, value), input.MaxValue);
        }

        private static Bitmap ScaleToFit(Bitmap src, int maxWidth, int maxHeight)
        {
            if (src == null)
                return null;
            int w = src.Width, h = src.Height;
            double scale = Math.Min(1.0, Math.Min((double)maxWidth / w, (double)maxHeight / h));
            if (scale >= 1.0)
                return src;

            int nw = Math.Max(1, (int)(w * scale));
            int nh = Math.Max(1, (int)(h * scale));
            var dst = new Bitmap(nw, nh, PixelFormat.Format32bppRgb);
            using (var g = new Graphics(dst))
            {
                g.DrawImage(src, new RectangleF(0, 0, nw, nh));
            }
            return dst;
        }

        private static Bitmap Invert(Bitmap src, int firstX, int firstY, int secondX, int secondY)
        {
            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppRgb);

            using (var g = new Graphics(dst))
                g.DrawImage(src, new RectangleF(0, 0, src.Width, src.Height));

            using (var data = dst.Lock())
            {
                unsafe
                {
                    byte* p = (byte*)data.Data;
                    int stride = data.ScanWidth;
                    int left = Math.Min(firstX, secondX);
                    int right = Math.Max(firstX, secondX);
                    int top = Math.Min(firstY, secondY);
                    int bottom = Math.Max(firstY, secondY);

                    for (int y = top; y <= bottom; y++)
                    {
                        byte* row = p + y * stride;
                        for (int x = left; x <= right; x++)
                        {
                            int i = x * 4;
                            row[i + 0] = (byte)(255 - row[i + 0]); // B
                            row[i + 1] = (byte)(255 - row[i + 1]); // G
                            row[i + 2] = (byte)(255 - row[i + 2]); // R
                        }
                    }
                }
            }

            return dst;
        }
    }
}
