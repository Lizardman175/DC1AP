using System.Collections.Generic;

namespace DC1AP.Logging
{
    public class APMessageModel
    {
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
        public int Type { get; set; }
        public Color Color { get; set; }
        public bool IsBackgroundColor { get; set; }
    }

    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
}
