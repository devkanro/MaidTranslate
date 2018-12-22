using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Logging;
using Kanro.MaidTranslate.Hook;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate.Translation
{
    public class TextTranslation
    {
        public TextTranslation(NameTextTranslationPool namePool)
        {
            NamePool = namePool;
        }

        public NameTextTranslationPool NamePool { get; }

        public string Original { get; set; }

        public Regex Regex { get; set; }

        public string Translation { get; set; }

        public TextTranslationFlag Flag { get; set; } = TextTranslationFlag.None;

        public HashSet<string> Scene { get; } = new HashSet<string>();

        public HashSet<int> Level { get; } = new HashSet<int>();

        public HashSet<string> ObjectName { get; } = new HashSet<string>();

        public HashSet<string> ObjectId { get; } = new HashSet<string>();

        public HashSet<TextType> Type { get; } = new HashSet<TextType>();

        public HashSet<TextSource> Source { get; } = new HashSet<TextSource>();

        public bool IsValid
        {
            get
            {
                if ((Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
                {
                    if (Regex == null) return false;
                }
                else
                {
                    if (Original == null) return false;
                }

                if (string.IsNullOrEmpty(Translation)) return false;

                return true;
            }
        }

        public bool Translate(Scene? scene, object source, TextTranslationEventArgs e, string original, out string translate)
        {
            if (Type.Count > 0)
            {
                if (!Type.Contains(e.Type))
                {
                    translate = null;
                    return false;
                }
            }

            if (Source.Count > 0)
            {
                if (!Source.Contains(e.Source))
                {
                    translate = null;
                    return false;
                }
            }

            if (Scene.Count > 0)
            {
                if (!Scene.Contains(scene?.name))
                {
                    translate = null;
                    return false;
                }
            }

            if (Scene.Count > 0)
            {
                if (!Scene.Contains(scene?.name))
                {
                    translate = null;
                    return false;
                }
            }

            if (Level.Count > 0)
            {
                if (!Level.Contains(scene?.buildIndex ?? -1))
                {
                    translate = null;
                    return false;
                }
            }

            var objectName = (source as UnityEngine.Object)?.name;
            if (ObjectName.Count > 0)
            {
                if (!ObjectName.Contains(objectName))
                {
                    translate = null;
                    return false;
                }
            }

            var objectId = (source as UnityEngine.Object)?.GetInstanceID();
            if (ObjectId.Count > 0)
            {
                if (!ObjectId.Contains(objectId?.ToString()))
                {
                    translate = null;
                    return false;
                }
            }

            if ((Flag & TextTranslationFlag.TextPart) > TextTranslationFlag.None)
            {
                if ((Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
                {
                    if (!Regex.IsMatch(original))
                    {
                        translate = null;
                        return false;
                    }

                    translate = Regex.Replace(original, Translation);
                    if (e.Debug) Logger.Log(LogLevel.Info, $"[TranslateDebug] TextPart {original} >> {translate}");
                    return true;
                }
                else
                {
                    if (original != Original)
                    {
                        translate = null;
                        return false;
                    }

                    translate = Translation;
                    if (e.Debug) Logger.Log(LogLevel.Info, $"[TranslateDebug] TextPart {original} >> {translate}");
                    return true;
                }
            }
            else
            {
                if ((Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
                {
                    var matches = Regex.Matches(original);

                    if (matches.Count == 0)
                    {
                        translate = null;
                        return false;
                    }

                    var originalTemp = new StringBuilder(original, 32);

                    foreach (Match match in matches)
                    {
                        var raw = match.Value;
                        var translateTemp = new StringBuilder(Translation, 32);

                        for (var i = match.Groups.Count - 1; i >= 0; i--)
                        {
                            var group = match.Groups[i];
                            if (!NamePool.Translate(scene, source, e, group.Value, out var groupValue))
                            {
                                groupValue = group.Value;
                            }

                            translateTemp = translateTemp.Replace($"${i}", groupValue);
                            translateTemp = translateTemp.Replace($"${{{i}}}", groupValue);
                        }

                        foreach (var name in Regex.GetGroupNames())
                        {
                            var group = match.Groups[name];
                            if (!NamePool.Translate(scene, source, e, group.Value, out var groupValue))
                            {
                                groupValue = group.Value;
                            }

                            translateTemp = translateTemp.Replace($"${{{name}}}", groupValue);
                        }

                        originalTemp = originalTemp.Replace(raw, translateTemp.ToString(), match.Index, match.Length);
                    }

                    translate = originalTemp.ToString();
                    if (e.Debug) Logger.Log(LogLevel.Info, $"[TranslateDebug] {original} >> {translate}");
                    return true;
                }
                else
                {
                    if (original != Original)
                    {
                        translate = null;
                        return false;
                    }

                    translate = Translation;
                    if (e.Debug) Logger.Log(LogLevel.Info, $"[TranslateDebug] {original} >> {translate}");
                    return true;
                }
            }
        }

        public TextTranslation Clone()
        {
            var result = new TextTranslation(NamePool){
                Original = Original,
                Regex = Regex,
                Translation = Translation,
                Flag = Flag,
            };

            foreach (var value in Scene)
            {
                result.Scene.Add(value);
            }

            foreach (var value in Level)
            {
                result.Level.Add(value);
            }

            foreach (var value in ObjectName)
            {
                result.ObjectName.Add(value);
            }

            foreach (var value in ObjectId)
            {
                result.ObjectId.Add(value);
            }

            foreach (var value in Type)
            {
                result.Type.Add(value);
            }

            foreach (var value in Source)
            {
                result.Source.Add(value);
            }

            return result;
        }
    }
}