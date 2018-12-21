using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kanro.MaidTranslate.Hook;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate.Translation
{
    public class TextTranslation
    {
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

            if ((Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
            {
                if (!Regex.IsMatch(original))
                {
                    translate = null;
                    return false;
                }

                translate = Regex.Replace(original, Translation);
                return true;
            }
            else if ((Flag & TextTranslationFlag.TextPart) > TextTranslationFlag.TextPart)
            {
                if (!original.Contains(Original))
                {
                    translate = null;
                    return false;
                }

                translate = original.Replace(Original, Translation);
                return true;
            }
            else if (Original == original)
            {
                translate = Translation;
                return true;
            }

            translate = null;
            return false;
        }
    }
}