using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kanro.MaidTranslate.Hook
{
    public class YotogiKagHitRetEventArgs : EventArgs
    {
        public string Voice { get; }
        public string Text { get; set; }
        public string Translation { get; set; }
        public List<KagTagSupport> TagCallStack { get; } = new List<KagTagSupport>();

        public YotogiKagHitRetEventArgs(string voice)
        {
            this.Voice = voice;
        }
    }
}
