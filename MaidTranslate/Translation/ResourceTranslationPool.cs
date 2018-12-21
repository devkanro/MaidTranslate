using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace Kanro.MaidTranslate.Translation
{
    public class ResourceTranslationPool
    {
        public Dictionary<string, string> Resource { get; } = new Dictionary<string, string>();

        public void AddResource(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            if (Resource.ContainsKey(name))
            {
                Logger.Log(LogLevel.Debug, $"[MaidTranslate] Same name resource of {name} found, use the latest version.");
            }

            Resource[name] = path;
        }

        public bool Translate(Scene? scene, Texture2D texture)
        {
            if (Resource.ContainsKey(texture.name))
            {
                texture.LoadImage(File.ReadAllBytes(Resource[texture.name]));
                return true;
            }
            return false;
        }
    }
}
