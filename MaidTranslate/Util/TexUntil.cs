using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using UnityEngine;

namespace Kanro.MaidTranslate.Util
{
    public static class TexUntil
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("TexUntil");
        public static TextureResource LoadTexture(string name, byte[] data)
        {
            var binaryReader = new BinaryReader(new MemoryStream(data), Encoding.UTF8);
            var text = binaryReader.ReadString();
            if (text != "CM3D2_TEX")
            {
                Logger.Log(LogLevel.Error, $"[MaidTranslate] Wrong tex file for {name}.");
                return null;
            }
            var num = binaryReader.ReadInt32();
            var text2 = binaryReader.ReadString();
            var width = 0;
            var height = 0;
            var textureFormat = TextureFormat.ARGB32;
            Rect[] array = null;
            if (1010 <= num)
            {
                if (1011 <= num)
                {
                    int num2 = binaryReader.ReadInt32();
                    if (0 < num2)
                    {
                        array = new Rect[num2];
                        for (int i = 0; i < num2; i++)
                        {
                            float x = binaryReader.ReadSingle();
                            float y = binaryReader.ReadSingle();
                            float width2 = binaryReader.ReadSingle();
                            float height2 = binaryReader.ReadSingle();
                            array[i] = new Rect(x, y, width2, height2);
                        }
                    }
                }
                width = binaryReader.ReadInt32();
                height = binaryReader.ReadInt32();
                textureFormat = (TextureFormat)binaryReader.ReadInt32();
            }
            var num3 = binaryReader.ReadInt32();
            var array2 = new byte[num3];
            binaryReader.Read(array2, 0, num3);

            if (num == 1000)
            {
                width = ((int)array2[16] << 24 | (int)array2[17] << 16 | (int)array2[18] << 8 | (int)array2[19]);
                height = ((int)array2[20] << 24 | (int)array2[21] << 16 | (int)array2[22] << 8 | (int)array2[23]);
            }

            binaryReader.Close();
            return new TextureResource(width, height, textureFormat, array, array2);
        }
    }
}
