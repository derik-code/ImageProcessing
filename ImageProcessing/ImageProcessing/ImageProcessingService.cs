using Eto.Drawing;

namespace ImageInverse
{
    public sealed class ImageProcessingService
    {
        public Bitmap Process(
            Bitmap source,
            ImageProcessingMode mode,
            ImageRegion region,
            int constant)
        {
            switch (mode)
            {
                case ImageProcessingMode.Invert:
                    return ImageProcessingTools.Invert(
                        source,
                        region.FirstX,
                        region.FirstY,
                        region.SecondX,
                        region.SecondY);

                case ImageProcessingMode.Grayscale:
                    return ImageProcessingTools.Grayscale(
                        source,
                        region.FirstX,
                        region.FirstY,
                        region.SecondX,
                        region.SecondY);

                case ImageProcessingMode.Lighten:
                    return ImageProcessingTools.Lighten(
                        source,
                        region.FirstX,
                        region.FirstY,
                        region.SecondX,
                        region.SecondY,
                        constant);

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}