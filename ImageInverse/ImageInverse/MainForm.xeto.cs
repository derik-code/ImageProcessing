using System;
using Eto.Forms;
using Eto.Drawing;
using System.Collections;

namespace ImageInverse
{
    public class MainForm : Form
    {
        private readonly Size maxImageSize = new Size(380, 380);
        private Bitmap? selectedBitmap;
        private NumericStepper firstXInput;
        private NumericStepper firstYInput;
        private NumericStepper secondXInput;
        private NumericStepper secondYInput;
        private NumericStepper constant;
        private NumericStepper increment;
        private ImageView imageOriginal;
        private ImageView imageResult;
        private Bitmap? bitmapResult;
        private Label constantLabel;
        private Label incrementLabel;
        private readonly ImageProcessingService processingService =
            new ImageProcessingService();

        private ImageProcessingMode currentMode =
            ImageProcessingMode.Invert;

        public MainForm()
        {
            Title = "Image Processing";
            ClientSize = new Size(820, 640);

            Menu = CreateMenu();

            imageOriginal = new ImageView { Size = maxImageSize };
            imageResult = new ImageView { Size = maxImageSize };

            var selectImageButton = new Button
            {
                Text = "Select",
                Command = new Command((s, e) => SelectImage()),
                Size = new Size(120, 32)
            };

            var processButton = new Button
            {
                Text = "Process",
                Command = new Command((s, e) => ProcessImage()),
                Size = new Size(120, 32)
            };

            Button saveResultButton = new Button
            {
                Text = "Save",
                Command = new Command((s, e) => SaveImage()),
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
                    processButton,
                    saveResultButton
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
                }
            };

            var thirdPointRow = new StackLayout {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Items = {
                    constantLabel,
                    constant,

                    incrementLabel,
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
            // { topBar, firstPointRow, secondPointRow, thirdPointRow, imagesTable }
            Content = new StackLayout
            {
                Padding = 10,
                Spacing = 10,
                Items = { topBar, firstPointRow, secondPointRow, thirdPointRow, imagesTable }
            };

            SetMode(ImageProcessingMode.Invert);
        }

        private void SetMode(ImageProcessingMode mode)
        {
            currentMode = mode;

            bool showLightenOptions =
                mode == ImageProcessingMode.Lighten;

            constantLabel.Visible = showLightenOptions;
            constant.Visible = showLightenOptions;
            incrementLabel.Visible = showLightenOptions;
            increment.Visible = showLightenOptions;
            switch (mode)
            {
                case ImageProcessingMode.Invert:
                    Title = "Image Processing (Invert Mode)";
                    break;
                case ImageProcessingMode.Grayscale:
                    Title = "Image Processing (Grayscale Mode)";
                    break;
                case ImageProcessingMode.Lighten:
                    Title = "Image Processing (Lighten Mode)";
                    break;
            }
        }

        private MenuBar CreateMenu()
        {
            var invertMode = new RadioMenuItem { Text = "Invert", Checked = true };
            var grayscaleMode = new RadioMenuItem { Text = "Grayscale" };
            var lightenMode = new RadioMenuItem { Text = "Lighten" };

            var quitButton = new ButtonMenuItem
            {
                Text = "Quit",
                Shortcut = Keys.Application | Keys.Q,
                Command = new Command((s, e) => HandleQuit(s, e))
            };

            var saveButton = new ButtonMenuItem {
                Text = "Save Image...",
                Shortcut = Keys.Control | Keys.S,
                Command = new Command((s, e) => SaveImage())
            };

            var loadButton = new ButtonMenuItem {
                Text = "Load Image...",
                Shortcut = Keys.Control | Keys.L,
                Command = new Command((s, e) => SelectImage())
            };

            var processButton = new ButtonMenuItem {
                Text = "Start process",
                Shortcut = Keys.Control | Keys.P,
                Command = new Command((s, e) => ProcessImage())
            };

            invertMode.Click += (s, e) => SetMode(ImageProcessingMode.Invert);
            grayscaleMode.Click += (s, e) => SetMode(ImageProcessingMode.Grayscale);
            lightenMode.Click += (s, e) => SetMode(ImageProcessingMode.Lighten);

            var modesMenu = new SubMenuItem
            {
                Text = "Modes",
                Items = { invertMode, grayscaleMode, lightenMode }
            };

            var fileMenu = new SubMenuItem
            {
                Text = "File",
                Items =
                {   
                    loadButton,
                    processButton,
                    saveButton,
                    quitButton
                }
            };

            var helpMenu = new SubMenuItem
            {
                Text = "Help",
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "About...",
                        Command = new Command((s, e) => HandleAbout(s, e))
                    }
                }
            };

            return new MenuBar
            {
                Items = { fileMenu, modesMenu, helpMenu }
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
                Width = 160
            };
        }

        protected void HandleAbout(object sender, EventArgs e)
        {
            new AboutDialog()
            {
                Version = "2.0.1",
                Developers = new[] { "Dinar Dusov MO-401B" },
                ProgramDescription = "Program for image processing. Powered by C# and Eto.",
                ProgramName = "Image Processing"
            }.ShowDialog(this);
        }

        protected void HandleQuit(object sender, EventArgs e)
        {
            Application.Instance.Quit();
        }

        private void SelectImage()
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

        private static Bitmap? ScaleToFit(Bitmap? src, int maxWidth, int maxHeight)
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

        private void ProcessImage()
        {
            if (selectedBitmap == null)
            {
                MessageBox.Show(this, "Select an image first");
                return;
            }

            var region = new ImageRegion(
                (int)firstXInput.Value,
                (int)firstYInput.Value,
                (int)secondXInput.Value,
                (int)secondYInput.Value);

            bitmapResult = processingService.Process(
                selectedBitmap,
                currentMode,
                region,
                (int)constant.Value);

            imageResult.Image = ScaleToFit(
                bitmapResult,
                maxImageSize.Width,
                maxImageSize.Height);
        }

        private void SaveImage()
        {
            if (bitmapResult == null)
            {
                MessageBox.Show(this, "Result is empty");
            }
            else
            {
                var dlg = new SaveFileDialog
                {
                    Filters =
                    {
                        new FileFilter("PNG", "*.png"),
                        new FileFilter("JPEG", "*.jpg"),
                        new FileFilter("BMP", "*.bmp")
                    }

                };
                if (dlg.ShowDialog(this) != DialogResult.Ok)
                {
                    MessageBox.Show(this, "Result not saved");
                }
                else
                {
                    string path = dlg.FileName;
                    string ext = System.IO.Path.GetExtension(path);
                    ImageFormat format = ext switch
                    {
                        ".png" => ImageFormat.Png,
                        ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                        ".bmp" => ImageFormat.Bitmap,
                        ".gif" => ImageFormat.Gif,
                        ".tif" or ".tiff" => ImageFormat.Tiff,
                        _ => ImageFormat.Png
                    };
                    bitmapResult.Save(path, format);
                    MessageBox.Show(this, "Result saved");
                }
            }
        }


    }


}
