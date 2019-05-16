using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using Kanro.MaidTranslate.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate
{
    public class ResourceDumper : IDisposable
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ResourceDumper");
        private static readonly string DumpDir = @"BepInEx\MaidTranslate\Dumps";
        private static readonly string TextDumpDir = $"{DumpDir}\\Text";
        private static readonly string TextureDumpDir = $"{DumpDir}\\Texture";
        private static readonly string UiDumpDir = $"{DumpDir}\\UI";
        private static readonly string SpriteDumpDir = $"{DumpDir}\\Sprite";
        private static readonly string ResourceDumpDir = $"{DumpDir}\\Resource";
        private static readonly string ObjectDumpDir = $"{DumpDir}\\Object";

        private HashSet<string> TextureDumpCache = new HashSet<string>();

        private HashSet<string> TextDumpCache = new HashSet<string>();

        private TranslateConfig _config;

        private TextWriter _textDumpingFile;

        private TextWriter TextDumpingFile
        {
            get
            {
                if (_textDumpingFile == null)
                {
                    var dumpFile = $"{TextDumpDir}\\{DateTime.Now:yyyy-MM-dd HH.mm.ss.fff}.txt";
                    Logger.Log(LogLevel.Debug, $"Text dumping started in '{dumpFile}'.");
                    _textDumpingFile = File.CreateText(dumpFile);
                }

                return _textDumpingFile;
            }
        }

        public ResourceDumper(TranslateConfig config)
        {
            _config = config;
            Directory.CreateDirectory(TextDumpDir);
            Directory.CreateDirectory(TextureDumpDir);
            Directory.CreateDirectory(UiDumpDir);
            Directory.CreateDirectory(SpriteDumpDir);
            Directory.CreateDirectory(ResourceDumpDir);
            Directory.CreateDirectory(ObjectDumpDir);
        }

        public void DumpText(Scene? scene, object source, TextTranslationEventArgs e)
        {
            if (!_config.IsDumpingText)
            {
                return;
            }

            var key = scene != null ? $"[{scene.Value.name}]{e.Text}" : e.Text;

            if (TextDumpCache.Contains(key))
            {
                return;
            }

            if (_config.DumpSkippedText.IsMatch(e.Text))
            {
                return;
            }

            var objectName = (source as UnityEngine.Object)?.name;
            var objectId = (source as UnityEngine.Object)?.GetInstanceID();

            if (objectId == 178854 && objectName == "Message" && e.Type == TextType.UiLabel)
            {
                // Skip for adv text
                return;
            }

            lock (TextDumpingFile)
            {   

                TextDumpingFile.WriteLine($"*type:{e.Type}");
                TextDumpingFile.WriteLine($"*source:{e.Source}");
                if (scene?.name != null)
                {
                    TextDumpingFile.WriteLine($"*scene:{scene?.name}");
                }

                if (scene?.buildIndex != null)
                {
                    TextDumpingFile.WriteLine($"*level:{scene?.buildIndex}");
                }

                if (objectName != null)
                {
                    TextDumpingFile.WriteLine($"*objectName:{objectName}");
                }

                if (objectId != null)
                {
                    TextDumpingFile.WriteLine($"*objectId:{objectId}");
                }

                TextDumpingFile.WriteLine($">{e.Text.Escape()}");
                TextDumpingFile.WriteLine($"<");
                TextDumpingFile.WriteLine();

                TextDumpingFile.Flush();
            }

            TextDumpCache.Add(key);
        }

        public void DumpResource(Scene? scene)
        {
            if (scene == null)
            {
                return;
            }

            var scenePath = Path.Combine(ResourceDumpDir, SceneManager.GetActiveScene().name);
            Directory.CreateDirectory(scenePath);

            var textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D));

            foreach (var texture in textures)
            {
                try
                {
                    var targetFile = Path.Combine(scenePath, texture.name + ".png");
                    if (File.Exists(targetFile))
                    {
                        continue;
                    }

                    DumpTexture2D((Texture2D)texture, Path.Combine(scenePath, texture.name + ".png"));
                    Logger.Log(LogLevel.Debug, $"Resource '{texture.name}' dumped.");
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, ex);
                }
            }
        }

        public bool DumpTexture(Scene? scene, ArcTextureLoadedEventArgs e)
        {
            if (!_config.IsDumpingTexture)
            {
                return false;
            }

            if (scene == null)
            {
                return false;
            }

            var key = $"Texture:{e.Name}";
            if (TextureDumpCache.Contains($"Texture:{e.Name}"))
            {
                return false;
            }

            if (e.Name.StartsWith("!"))
            {
                return false;
            }

            var texture = e.Data.CreateTexture2D();
            DumpTexture2D(texture, Path.Combine(TextureDumpDir, e.Name + ".png"));
            if (!_config.ForceDumping)
            {
                TextureDumpCache.Add(key);
            }
            return true;
        }

        public bool DumpUI(Scene? scene, TextureTranslationEventArgs e)
        {
            if (!_config.IsDumpingUI)
            {
                return false;
            }

            if (scene == null)
            {
                return false;
            }

            var key = $"UI:{e.Name}";
            if (TextureDumpCache.Contains($"UI:{e.Name}"))
            {
                return false;
            }

            if (e.Name.StartsWith("!"))
            {
                return false;
            }

            var texture = e.OriginalTexture ?? e.Translation.CreateTexture2D();
            DumpTexture2D(texture, Path.Combine(UiDumpDir, texture.name + ".png"));
            if (!_config.ForceDumping)
            {
                TextureDumpCache.Add(key);
            }
            return true;
        }

        public bool DumpSprite(Scene? scene, TextureTranslationEventArgs e)
        {
            if (!_config.IsDumpingSprite)
            {
                return false;
            }

            if (scene == null)
            {
                return false;
            }

            var key = $"Sprite:{e.Name}";
            if (TextureDumpCache.Contains($"Sprite:{e.Name}"))
            {
                return false;
            }

            if (e.Name.StartsWith("!"))
            {
                return false;
            }

            var texture = e.OriginalTexture ?? e.Translation.CreateTexture2D();
            DumpTexture2D(texture, Path.Combine(SpriteDumpDir, texture.name + ".png"));
            if (!_config.ForceDumping)
            {
                TextureDumpCache.Add(key);
            }
            return true;
        }

        private void DumpTexture2D(Texture2D texture, string path)
        {
            var tmp = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            Graphics.Blit(texture, tmp);
            var previous = RenderTexture.active;
            RenderTexture.active = tmp;
            var readableTexture2D = new Texture2D(texture.width, texture.height);
            readableTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            readableTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            var bytes = readableTexture2D.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
        }
        
        public void DumpObjects(Scene? scene)
        {
            if (scene == null)
            {
                return;
            }

            var currentScene = scene.Value;
            var objects = currentScene.GetRootGameObjects();

            using (var f = File.OpenWrite(Path.Combine(ObjectDumpDir, $"{currentScene.name}.txt")))
            {
                using (var sw = new StreamWriter(f, Encoding.UTF8))
                {
                    foreach (var obj in objects)
                    {
                        PrintRecursive(sw, obj);
                    }
                }
            }
            Process.Start("notepad.exe", Path.Combine(ObjectDumpDir, $"{currentScene.name}.txt"));
        }

        private static void PrintRecursive(StreamWriter sw, GameObject obj)
        {
            PrintRecursive(sw, obj, 0);
        }

        private static void PrintRecursive(StreamWriter sw, GameObject obj, int d)
        {
            var pad1 = new string(' ', 3 * d);
            var pad2 = new string(' ', 3 * (d + 1));
            var pad3 = new string(' ', 3 * (d + 2));
            sw.WriteLine(pad1 + obj.name + "--" + obj.GetType().FullName);

            foreach (Component c in obj.GetComponents<Component>())
            {
                sw.WriteLine(pad2 + "::" + c.GetType().Name);

                var ct = c.GetType();
                var props = ct.GetProperties(System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                foreach (var p in props)
                {
                    try
                    {
                        var v = p.GetValue(c, null);
                        sw.WriteLine(pad3 + "@" + p.Name + "<" + p.PropertyType.Name + "> = " + v);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(LogLevel.Debug, e);
                    }
                }

            }
            foreach (Transform t in obj.transform)
            {
                PrintRecursive(sw, t.gameObject, d + 1);
            }
        }


        public void Dispose()
        {
            _textDumpingFile?.Flush();
            _textDumpingFile?.Dispose();
        }
    }
}
