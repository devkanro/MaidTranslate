using System;

namespace Kanro.MaidTranslate.Hook
{
    public class TextTranslationEventArgs : EventArgs
    {
        public bool Debug { get; set; } = false;
        public TextSource Source { get; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public TextType Type { get;  }
        public TextTranslationEventArgs(String text, TextType type = TextType.Unknown, TextSource source = TextSource.Unknown)
        {
            Text = text;
            Type = type;
            Source = source;
        }
    }
}
