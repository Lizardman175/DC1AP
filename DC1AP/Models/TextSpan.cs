using Avalonia.Media;

namespace DC1AP.Models
{
    public class TextSpan
    {
        private string _text;
        public string Text { get { return _text; } set
            {
                _text = value.Trim() + " ";
            } }
        public IBrush TextColor { get; set; }
    }
}
