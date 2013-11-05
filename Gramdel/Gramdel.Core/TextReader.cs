using System;

namespace Gramdel.Core
{
    public class TextReader
    {
        public int SkipSpaces(string code, int position)
        {
            for (int pt1 = 0; ; pt1++)
            {
                if (pt1 + position > code.Length)
                    return 0;

                if (!char.IsWhiteSpace(code[position + pt1]))
                    return pt1;
            }
        }

        public bool TryReadToken(string code, int position, string token)
        {
            for (int pt1 = 0; pt1 < token.Length; pt1++)
            {
                if (pt1 + position > code.Length)
                    return false;

                if (code[position + pt1] != token[pt1])
                    return false;
            }

            return true;
        }

        public string ReadText(string code, int position, Func<char, bool> predicate)
        {
            for (int pt1 = 0; ; pt1++)
            {
                if (pt1 + position > code.Length)
                    return code.Substring(position, pt1);

                if (!predicate(code[position + pt1]))
                    return code.Substring(position, pt1);
            }
        }
    }
}
