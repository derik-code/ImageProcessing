using Eto.Drawing;

namespace ImageInverse
{
public static class ImageProcessingTools
{
    public static Bitmap Invert(Bitmap src, int firstX, int firstY, int secondX, int secondY)
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

    public static Bitmap Grayscale(Bitmap src, int firstX, int firstY, int secondX, int secondY)
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
                        byte r = row[i + 0];
                        byte g = row[i + 1];
                        byte b = row[i + 2];

                        byte gray = (byte)((r + g + b) / 3);

                        row[i + 0] = gray;
                        row[i + 1] = gray;
                        row[i + 2] = gray;     
                    }
                }
            }
        }

        return dst;
    }

    public static Bitmap Lighten(Bitmap src, int firstX, int firstY, int secondX, int secondY, int constant)
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
                        row[0 + i] = (byte)Math.Clamp(row[0 + i] + constant, 0, 255);
                        row[1 + i] = (byte)Math.Clamp(row[1 + i] + constant, 0, 255);
                        row[2 + i] = (byte)Math.Clamp(row[2 + i] + constant, 0, 255);
                    }
                }
            }
        }

        return dst;
    }
}
}
