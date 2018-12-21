using System;

namespace Kanro.MaidTranslate.Translation
{
    [Flags]
    public enum TextTranslationFlag
    {
        None = 0,
        Regex = 1,
        TextPart = 1 << 1
    }
}