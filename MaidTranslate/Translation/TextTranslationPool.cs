using System.Collections.Generic;
using Kanro.MaidTranslate.Hook;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate.Translation
{
    public class TextTranslationPool
    {
        private Dictionary<string, HashSet<TextTranslation>> RawSpacial { get; } =
            new Dictionary<string, HashSet<TextTranslation>>();
        private HashSet<TextTranslation> RegexSpacial { get; } = new HashSet<TextTranslation>();
        public NameTextTranslationPool NamePool { get; } = new NameTextTranslationPool();

        public bool Translate(Scene? scene, object source, TextTranslationEventArgs e, out string translate)
        {
            if (NamePool.Translate(scene, source, e, e.Text, out var result))
            {
                translate = result;
                return true;
            }

            if (RawSpacial.TryGetValue(e.Text, out var translations))
            {
                if (translations.Count > 0)
                {
                    foreach (var textTranslation in translations)
                    {
                        if (!textTranslation.Translate(scene, source, e, e.Text, out result)) continue;

                        translate = result;
                        return true;
                    }
                }
            }

            foreach (var textTranslation in RegexSpacial)
            {
                if (!textTranslation.Translate(scene, source, e, e.Text, out result)) continue;

                translate = result;
                return true;
            }

            translate = null;
            return false;
        }

        public void AddTranslation(TextTranslation translation)
        {
            if ((translation.Flag & TextTranslationFlag.TextPart) > TextTranslationFlag.None)
            {
                NamePool.AddTranslation(translation);
            }
            else
            {
                if ((translation.Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
                {
                    RegexSpacial.Add(translation);
                }
                else
                {
                    if (RawSpacial.ContainsKey(translation.Original))
                    {
                        RawSpacial[translation.Original]?.Add(translation);
                    }
                    else
                    {
                        RawSpacial[translation.Original] = new HashSet<TextTranslation>() { translation };
                    }
                }
            }
        }
    }
}