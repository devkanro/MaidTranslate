using System.Collections.Generic;
using Kanro.MaidTranslate.Hook;
using UnityEngine.SceneManagement;

namespace Kanro.MaidTranslate.Translation
{
    public class NameTextTranslationPool
    {
        private HashSet<TextTranslation> RawSpacial { get; } = new HashSet<TextTranslation>();
        private HashSet<TextTranslation> RegexSpacial { get; } = new HashSet<TextTranslation>();

        public virtual bool Translate(Scene? scene, object source, TextTranslationEventArgs e, out string translate)
        {
            var original = e.Text;
            string result = null;
            var flag = false;
            
            foreach (var textTranslation in RawSpacial)
            {
                if (!textTranslation.Translate(scene, source, e, original, out result)) continue;

                flag = true;
                original = result;
            }

            foreach (var textTranslation in RegexSpacial)
            {
                if (!textTranslation.Translate(scene, source, e, original, out result)) continue;

                flag = true;
                original = result;
            }

            translate = flag ? original : null;
            return flag;
        }

        public virtual void AddTranslation(TextTranslation translation)
        {
            if ((translation.Flag & TextTranslationFlag.Regex) > TextTranslationFlag.None)
            {
                RegexSpacial.Add(translation);
            }
            else
            {
                RegexSpacial.Add(translation);
            }
        }
    }
}