using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using Kanro.MaidTranslate.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = BepInEx.Logger;

namespace Kanro.MaidTranslate.Translation
{
    public class TranslationResource
    {
        private static readonly string TranslationDir = @"BepInEx\MaidTranslate";
        private static readonly string TextTranslationDir = $"{TranslationDir}\\Text";
        private static readonly string TextureTranslationDir = $"{TranslationDir}\\Texture";
        private static readonly string UITranslationDir = $"{TranslationDir}\\UI";
        private static readonly string SpriteTranslationDir = $"{TranslationDir}\\Sprite";
        private static readonly string ResourceTranslationDir = $"{TranslationDir}\\Resource";
        private static readonly string TranslationPackDir = $"{TranslationDir}\\Pack";

        private static readonly Regex EmptyStringRegex = new Regex(@"^\s*$", RegexOptions.Compiled);

        public TextTranslationPool TextTranslationPool { get; private set; }
        public ResourceTranslationPool ResourceTranslationPool { get; private set; }
        public TextureTranslationPool TextureTranslationPool { get; private set; }
        public TextureTranslationPool SpriteTranslationPool { get; private set; }
        public TextureTranslationPool UITranslationPool { get; private set; }
        public AppDomain TranslatePackDomain { get; private set; }

        public TranslationResource()
        {
            Directory.CreateDirectory(TextTranslationDir);
            Directory.CreateDirectory(TextureTranslationDir);
            Directory.CreateDirectory(UITranslationDir);
            Directory.CreateDirectory(SpriteTranslationDir);
            Directory.CreateDirectory(ResourceTranslationDir);
            Directory.CreateDirectory(TranslationPackDir);

            Reload();
        }

        public void Reload()
        {
            LoadTextTranslation();
            LoadResourceTranslation();
            LoadTextureTranslation();
            LoadUITranslation();
            LoadSpriteTranslation();
            LoadTranslationPack();
        }

        public bool TranslateText(Scene? scene, object source, TextTranslationEventArgs e, out string translate)
        {
            if (TextTranslationPool.Translate(scene, source, e, out var temp))
            {
                translate = temp;
                return true;
            }

            translate = temp;
            return false;
        }

        public bool TranslateResource(Scene? scene, Texture2D texture)
        {
            return ResourceTranslationPool.Translate(scene, texture);
        }
        
        public bool TranslateTexture(Scene? scene, TextureTranslationEventArgs e, out TextureResource resource)
        {
            return TextureTranslationPool.Translate(scene, e, out resource);
        }

        public bool TranslateUI(Scene? scene, TextureTranslationEventArgs e, out TextureResource resource)
        {
            return UITranslationPool.Translate(scene, e, out resource);
        }

        public bool TranslateSprite(Scene? scene, TextureTranslationEventArgs e, out TextureResource resource)
        {
            return SpriteTranslationPool.Translate(scene, e, out resource);
        }

        public void LoadTranslationPack()
        {
        }

        public void LoadTextTranslation()
        {
            var textPool = new TextTranslationPool();
            var count = 0;

            var translationFiles = Directory.GetFiles(TextTranslationDir, "*.txt", SearchOption.AllDirectories);

            foreach (var file in translationFiles)
            {
                var lines = File.ReadAllLines(file);
                var baseTranslation = new TextTranslation(textPool.NamePool);
                var translation = baseTranslation.Clone();

                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line) || EmptyStringRegex.IsMatch(line))
                    {
                        if (translation.IsValid)
                        {
                            textPool.AddTranslation(translation);
                            count++;
                        }
                        translation = baseTranslation.Clone();
                        continue;
                    }

                    switch (line[0])
                    {
                        case '#':
                            break;
                        case '&':
                            ConfigTranslationSetting(baseTranslation, line.Substring(1));
                            ConfigTranslationSetting(translation, line.Substring(1));
                            break;
                        case '%':
                            baseTranslation = new TextTranslation(textPool.NamePool);
                            break;
                        case '*':
                            ConfigTranslationSetting(translation, line.Substring(1));
                            break;
                        case '-':
                            translation.Original = line.Substring(1).Unescape();
                            translation.Flag |= TextTranslationFlag.TextPart;
                            break;
                        case '+':
                            translation.Regex = new Regex($"^{line.Substring(1)}$", RegexOptions.Compiled);
                            translation.Flag |= TextTranslationFlag.Regex;
                            translation.Flag |= TextTranslationFlag.TextPart;
                            break;
                        case '>':
                            translation.Original = line.Substring(1).Unescape();
                            break;
                        case '$':
                            translation.Regex = new Regex($"^{line.Substring(1)}$", RegexOptions.Compiled);
                            translation.Flag |= TextTranslationFlag.Regex;
                            break;
                        case '<':
                            translation.Translation = line.Substring(1).Unescape();
                            break;
                        default:
                            break;
                    }
                }
                
                if (translation.IsValid)
                {
                    textPool.AddTranslation(translation);
                    count++;
                }
            }

            TextTranslationPool = textPool;
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] {count} text translation loaded.");
        }

        private void ConfigTranslationSetting(TextTranslation translation, string value)
        {
            var data = value.Split(new[] { ':' }, 2);
            if (data.Length == 2)
            {
                switch (data[0])
                {
                    case "type":
                        foreach (var type in data[1].Split(','))
                        {
                            try
                            {
                                translation.Type.Add(
                                    (TextType)Enum.Parse(typeof(TextType), type, true));
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                        break;
                    case "source":
                        foreach (var source in data[1].Split(','))
                        {
                            try
                            {
                                translation.Source.Add(
                                    (TextSource)Enum.Parse(typeof(TextSource), source, true));
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                        break;
                    case "scene":
                        foreach (var scene in data[1].Split(','))
                        {
                            translation.Scene.Add(scene);
                        }
                        break;
                    case "level":
                        foreach (var level in data[1].Split(','))
                        {
                            if (int.TryParse(level, out var result))
                            {
                                translation.Level.Add(result);
                            }
                        }
                        break;
                    case "objectName":
                        foreach (var name in data[1].Split(','))
                        {
                            translation.ObjectName.Add(name);
                        }
                        break;
                    case "objectId":
                        foreach (var id in data[1].Split(','))
                        {
                            translation.ObjectId.Add(id);
                        }
                        break;
                }
            }
        }

        public void LoadResourceTranslation()
        {
            var resourcePool = new ResourceTranslationPool();
            var textureFiles = Directory.GetFiles(ResourceTranslationDir, "*.png", SearchOption.AllDirectories);

            foreach (var file in textureFiles)
            {
                resourcePool.AddResource(file);
            }

            ResourceTranslationPool = resourcePool;
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] {resourcePool.Resource.Count} resource translation loaded.");
        }

        public void LoadTextureTranslation()
        {
            var resourcePool = new TextureTranslationPool();
            var textureFiles = Directory.GetFiles(TextureTranslationDir, "*.*", SearchOption.AllDirectories)
                .Where(s => s.EndsWith(".png") || s.EndsWith(".tex")).ToList();

            foreach (var file in textureFiles)
            {
                resourcePool.AddResource(file);
            }

            TextureTranslationPool = resourcePool;
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] {resourcePool.Resource.Count} texture translation loaded.");
        }

        public void LoadUITranslation()
        {
            var resourcePool = new TextureTranslationPool();
            var textureFiles = Directory.GetFiles(UITranslationDir, "*.png", SearchOption.AllDirectories);

            foreach (var file in textureFiles)
            {
                resourcePool.AddResource(file);
            }

            UITranslationPool = resourcePool;
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] {resourcePool.Resource.Count} UI translation loaded.");
        }

        public void LoadSpriteTranslation()
        {
            var resourcePool = new TextureTranslationPool();
            var textureFiles = Directory.GetFiles(SpriteTranslationDir, "*.png", SearchOption.AllDirectories);

            foreach (var file in textureFiles)
            {
                resourcePool.AddResource(file);
            }

            SpriteTranslationPool = resourcePool;
            Logger.Log(LogLevel.Debug, $"[MaidTranslate] {resourcePool.Resource.Count} sprite translation loaded.");
        }
    }
}