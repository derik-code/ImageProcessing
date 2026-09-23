using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Eto.Serialization.Xaml;

namespace ImageInverse
{
	public class MainForm : Form
	{
		public MainForm()
		{
			XamlReader.Load(this);
			Title = "Image Inverse";
			ClientSize = new Size(800, 600);

			ImageView imageOriginal = new ImageView { Size = new Size(400, 400) };
			ImageView imageResult = new ImageView { Size = new Size(400, 400) };


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
					if (imageOriginal.Image != null)
					{
						imageResult.Image = Invert((Bitmap)imageOriginal.Image);
					}
				}),
				Size = new Size(120, 32)
			};

			Content = new TableLayout
			{
				Padding = 10,
				Rows =
				{
					new TableRow(
						new TableCell(new Label { Text = "Original" }, true),
						new TableCell(new Label { Text = "Result" }, true)
					),
					new TableRow(
						new TableCell(imageOriginal, true),
						new TableCell(imageResult, true)
					),
					new TableRow(
						new TableCell(
							new StackLayout
							{
								Orientation = Orientation.Horizontal,
								HorizontalContentAlignment = HorizontalAlignment.Center,
								Items = { selectImageButton }
							}, true),
						new TableCell(
							new StackLayout
							{
								Orientation = Orientation.Horizontal,
								HorizontalContentAlignment = HorizontalAlignment.Center,
								Items = { invertButton }
							}, true)
)
				}
			};
		}

		protected void HandleAbout(object sender, EventArgs e)
		{
			new AboutDialog().ShowDialog(this);
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
				var bmp = new Bitmap(dlg.FileName);
				imageOriginal.Image = bmp;
			}
		}

		private static Bitmap Invert(Bitmap src)
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

					for (int y = 0; y < dst.Height; y++)
					{
						byte* row = p + y * stride;
						for (int x = 0; x < dst.Width; x++)
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
