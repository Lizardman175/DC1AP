using DC1AP.Logging;
using DC1AP.Utils;
using DC1AP.ViewModels;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using Color = Avalonia.Media.Color;

namespace DC1AP.Models
{
    public class LogListItem : ViewModelBase
    {
        private ObservableCollection<TextSpan> _textSpans = new ObservableCollection<TextSpan>();

        public ObservableCollection<TextSpan> TextSpans
        {
            get => _textSpans;
            set => this.RaiseAndSetIfChanged(ref _textSpans, value);
        }

        public LogListItem(string text)
        {
            TextSpans = new ObservableCollection<TextSpan>()
            {
                new TextSpan(){Text = text},
            };
        }

        public LogListItem(string text, Color color)
        {
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                TextSpans = new ObservableCollection<TextSpan>()
                {
                    new TextSpan(){Text = text, TextColor = new SolidColorBrush(color)},
                };
            });

        }

        public LogListItem(IEnumerable<TextSpan> spans)
        {
            TextSpans = spans.ToObservableCollection();
        }

        public LogListItem(APMessageModel message)
        {
            TextSpans = new ObservableCollection<TextSpan>();
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                string text = "";
                string buffer = "";
                IBrush? bufferColor = null;
                foreach (var part in message.Parts)
                {
                    var span = new TextSpan
                    {
                        Text = part.Text
                    };

                    if (span.Text.Trim() == "(")
                    {
                        buffer = "(";
                    }
                    else if (span.Text.Trim() == ")")
                    {
                        span.Text = buffer + part.Text;
                        buffer = "";
                    }
                    else if (buffer != "")
                    {
                        buffer += span.Text.Trim();
                        bufferColor = new SolidColorBrush(Color.FromRgb((byte)part.Color.R, (byte)part.Color.G, (byte)part.Color.B));
                    }

                    if (buffer == "")
                    {
                        if (bufferColor != null)
                        {
                            span.TextColor = bufferColor;
                            bufferColor = null;
                        }
                        else
                            span.TextColor = new SolidColorBrush(Color.FromRgb((byte)part.Color.R, (byte)part.Color.G, (byte)part.Color.B));
                        TextSpans.Add(span);
                        text += part.Text;
                    }
                }
            });
        }
    }
}
