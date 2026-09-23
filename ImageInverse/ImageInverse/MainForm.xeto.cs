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
        private NumericStepper constant;
        private NumericStepper increment;
        public enum ImageProcessingMode
        {
            Invert,       // Инверсия цвета
            Grayscale,    // Оттенки серого
            Lighten       // Осветление
        }
        private ImageProcessingMode _currentMode = ImageProcessingMode.Invert;
        private ImageView imageOriginal;
        ImageView imageResult;
        Label constantLabel;
        Label incrementLabel;
        public MainForm()
        {
            XamlReader.Load(this);
            Title = "Image Inverse";
            ClientSize = new Size(820, 640);

            imageOriginal = new ImageView { Size = maxImageSize };
            imageResult = new ImageView { Size = maxImageSize };

            RadioMenuItem invertMode = new RadioMenuItem { Text = "Invert" };
            RadioMenuItem grayscaleMode = new RadioMenuItem { Text = "Grayscale" };
            RadioMenuItem lightenMode = new RadioMenuItem { Text = "Lighten" };
            invertMode.Checked = true;

            invertMode.Click += (s, e) => _currentMode = ImageProcessingMode.Invert;
            grayscaleMode.Click += (s, e) => _currentMode = ImageProcessingMode.Grayscale;
            lightenMode.Click += (s, e) =>
            {
                _currentMode = ImageProcessingMode.Lighten;
                switchLightenButtons();
            };

            SubMenuItem modMenu = new SubMenuItem
            {
                Text = "Mods",
                Items = { invertMode, grayscaleMode, lightenMode }
            };

            Menu = new MenuBar { Items = { modMenu } };

            Button selectImageButton = new Button
            {
                Text = "Select",
                Command = new Command((s, e) => SelectImage(imageOriginal, imageResult)),
                Size = new Size(120, 32)
            };

            Button processButton = new Button
            {
                Text = "Process",
                Command = new Command((s, e) => inverseButtonLogic()),
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
                    processButton
                }
            };

            firstXInput = CreateCoordinateInput();
            firstYInput = CreateCoordinateInput();
            secondXInput = CreateCoordinateInput();
            secondYInput = CreateCoordinateInput();
            increment = new NumericStepper { Value = 1, MinValue = 1, MaxValue = 25, Visible = false };
            constant = new NumericStepper { Value = 5, MinValue = 5, MaxValue = 255, Increment = 1, Visible = false };
            increment.ValueChanged += (s, e) => { constant.Increment = (int)increment.Value; };
            constantLabel = new Label { Text = "Const", VerticalAlignment = VerticalAlignment.Center, Visible = false };
            incrementLabel = new Label { Text = "Increment", VerticalAlignment = VerticalAlignment.Center, Visible = false };

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
                    secondYInput,

                    constant,

                    increment
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

        private bool lightenButtons = false;
        private void switchLightenButtons()
        {
            if (!lightenButtons)
            {
                incrementLabel.Visible = true;
                increment.Visible = true;
                constant.Visible = true;
                constantLabel.Visible = true;
            }
            else
            {
                incrementLabel.Visible = false;
                increment.Visible = false;
                constant.Visible = false;
                constantLabel.Visible = false;
            }
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
            new AboutDialog()
            {
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

        private void inverseButtonLogic()
        {
            if (selectedBitmap != null)
                switch (_currentMode)
                {
                    case ImageProcessingMode.Invert:
                        var inverted = ImageProcessingTools.Invert(
                            selectedBitmap,
                            (int)firstXInput.Value,
                            (int)firstYInput.Value,
                            (int)secondXInput.Value,
                            (int)secondYInput.Value);
                        imageResult.Image = ScaleToFit(inverted, maxImageSize.Width, maxImageSize.Height);

                        break;
                    case ImageProcessingMode.Grayscale:
                        var grayscaled = ImageProcessingTools.Grayscale(
                                selectedBitmap,
                                (int)firstXInput.Value,
                                (int)firstYInput.Value,
                                (int)secondXInput.Value,
                                (int)secondYInput.Value);
                        imageResult.Image = ScaleToFit(grayscaled, maxImageSize.Width, maxImageSize.Height);
                        break;
                    case ImageProcessingMode.Lighten:
                        var lighten = ImageProcessingTools.Lighten(
                            selectedBitmap,
                                (int)firstXInput.Value,
                                (int)firstYInput.Value,
                                (int)secondXInput.Value,
                                (int)secondYInput.Value,
                                (int)constant.Value);
                        imageResult.Image = ScaleToFit(lighten, maxImageSize.Width, maxImageSize.Height);
                        break;

                }
        }


    }


}
