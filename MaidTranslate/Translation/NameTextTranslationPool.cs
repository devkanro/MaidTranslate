using System.Collections.Generic;
using Kanro.MaidTranslate.Hook;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate.Translation
{
    public class NameTextTranslationPool
    {
        private HashSet<TextTranslation> RawSpacial { get; } = new HashSet<TextTranslation>();
        private HashSet<TextTranslation> RegexSpacial { get; } = new HashSet<TextTranslation>();

        public virtual bool Translate(Scene? scene, object source, TextTranslationEventArgs e, string original, out string translate)
        {
            string result;

            foreach (var textTranslation in RawSpacial)
            {
                if (!textTranslation.Translate(scene, source, e, original, out result)) continue;
                
                translate = result;
                return true;
            }

            foreach (var textTranslation in RegexSpacial)
            {
                if (!textTranslation.Translate(scene, source, e, original, out result)) continue;

                translate = result;
                return true;
            }

            translate = null;
            return false;
        }

        public virtual void AddTranslation(TextTranslation translation)
        {
            if ((translation.Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
            {
                RegexSpacial.Add(translation);
            }
            else
            {
                RawSpacial.Add(translation);
            }
        }
    }
}