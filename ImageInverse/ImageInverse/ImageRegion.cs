namespace ImageInverse
{
    public readonly struct ImageRegion
    {
        public int FirstX { get; }
        public int FirstY { get; }
        public int SecondX { get; }
        public int SecondY { get; }

        public ImageRegion(int firstX, int firstY, int secondX, int secondY)
        {
            FirstX = firstX;
            FirstY = firstY;
            SecondX = secondX;
            SecondY = secondY;
        }
    }
}