using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kanro.MaidTranslate.Util
{
    public static class Extensions
    {
        public static string Escape(this string txt)
        {
            StringBuilder stringBuilder = new StringBuilder(txt.Length + 2);
            foreach (char c in txt)
                switch (c)
                {
                    case '\0':
                        stringBuilder.Append(@"\0");
                        break;
                    case '\n':
                        stringBuilder.Append(@"\n");
                        break;
                    case '\r':
                        stringBuilder.Append(@"\r");
                        break;
                    case '\\':
                        stringBuilder.Append(@"\\");
                        break;
                    default:
                        stringBuilder.Append(c);
                        break;
                }
            return stringBuilder.ToString();
        }

        public static string Unescape(this string txt)
        {
            if (string.IsNullOrEmpty(txt))
                return txt;
            StringBuilder stringBuilder = new StringBuilder(txt.Length);
            for (int i = 0; i < txt.Length;)
            {
                int num = txt.IndexOf('\\', i);
                if (num < 0 || num == txt.Length - 1)
                    num = txt.Length;
                stringBuilder.Append(txt, i, num - i);
                if (num >= txt.Length)
                    break;
                char c = txt[num + 1];
                switch (c)
                {
                    case '0':
                        stringBuilder.Append('\0');
                        break;
                    case 'n':
                        stringBuilder.Append('\n');
                        break;
                    case 'r':
                        stringBuilder.Append('\r');
                        break;
                    case '\\':
                        stringBuilder.Append('\\');
                        break;
                    default:
                        stringBuilder.Append('\\').Append(c);
                        break;
                }
                i = num + 2;
            }
            return stringBuilder.ToString();
        }
    }
}
