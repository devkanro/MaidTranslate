using System;

namespace Kanro.MaidTranslate.Hook
{
    public class ArcTextureLoadedEventArgs : EventArgs
    {
        public string Name { get; }
        public TextureResource Data { get; set; }
        public ArcTextureLoadedEventArgs(String name, TextureResource data)
        {
            Name = name;
            Data = data;
        }
    }
}