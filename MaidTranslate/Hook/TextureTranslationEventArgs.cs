using System;
using UnityEngine;

namespace Kanro.MaidTranslate.Hook
{
    public class TextureTranslationEventArgs : EventArgs
    {
        public TextureType Type { get; }
        public string Name { get; }
        public Texture2D OriginalTexture { get; }
        public TextureResource Translation { get; set; }
        public TextureTranslationEventArgs(String name, TextureType type = TextureType.Unknown)
        {
            Name = name;
            Type = type;
        }

        public TextureTranslationEventArgs(String name, Texture2D originalTexture, TextureType type = TextureType.Unknown) : this(name, type)
        {
            OriginalTexture = originalTexture;
        }

        public TextureTranslationEventArgs(String name, TextureResource translation, TextureType type = TextureType.Unknown) : this(name, type)
        {
            Translation = translation;
        }
    }
}