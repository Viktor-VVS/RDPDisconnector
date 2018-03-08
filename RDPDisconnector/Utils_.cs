using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RDPDisconnector
{
    public class Utils_
    {
        public static void ActionWithGuiThreadInvoke(Control obj, Action _action)
        {
            obj.Dispatcher.Invoke(new Action(delegate
            {
                _action.Invoke();
            }
            ));
        }

        public static void ActionInThread(Action _action)
        {
            Task.Factory.StartNew(() => _action.Invoke());
        }
        /*
         using 
            Utils.ActionInThread( () =>
            {

            });
         */

    }
    public class StringsParser
    {
        public string Content { get; private set; }
        public string extractedString { get; private set; }
        public int movePosition { get; set; }
        public int extractPosition { get; set; }
        public int startPosition { get; set; }
        public StringsParser(string content)
        {
            Content = content;
            extractPosition = 0;
            movePosition = 0;
            extractedString = ""; // позиция которая нужна для Extract/вырезки
            startPosition = 0; //позиция с которой начнается каждый следующ поиск 

        }

        public bool backSearchTo(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                //startPosition = movePosition + findOccur.Length;
                startPosition = movePosition;
                return true;
            }
            else
                return false;
        }

        public bool backSearchTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.LastIndexOf(findOccur, 0, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)

            {
                //startPosition = movePosition + findOccur.Length;
                startPosition = movePosition;
                movePosition = startPosition + findOccur.Length;
                return true;
            }
            else
                return false;
        }

        // set cursor at the begin found findOccur
        public bool searchTo(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = movePosition + findOccur.Length;
                return true;
            }
            else
                return false;
        }
        // set cursor after found findOccur
        public bool searchTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((movePosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = movePosition + findOccur.Length;
                movePosition = startPosition;
                return true;
            }
            else
                return false;
        }
        public bool exctractTo(string findOccur, bool caseSensitive = true)
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                extractPosition = extractPosition + findOccur.Length;
                startPosition = extractPosition;
                extractedString = Content.Substring(movePosition, extractPosition - movePosition);
                return true;
            }
            else
                return false;
        }
        public bool exctractTo_NotIncluding(string findOccur, bool caseSensitive = true)
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)
            if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)) != -1)
            {
                startPosition = extractPosition + findOccur.Length;
                extractedString = Content.Substring(movePosition, extractPosition - movePosition);


                return true;
            }
            else
                return false;
        }

        public bool exctractToEnd()
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)

            extractPosition = Content.Length;
            startPosition = extractPosition;
            extractedString = Content.Substring(movePosition, extractPosition - movePosition);

            return true;
        }

        public bool exctractToEnd2()
        {
            //if ((extractPosition = Content.IndexOf(findOccur, startPosition, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)) != -1)

            extractPosition = Content.Length;
            extractedString = Content.Substring(startPosition, extractPosition - startPosition);

            return true;
        }

        public void replaceExtractedWith(string newContent)
        {
            int extractLen = extractPosition - movePosition;
            Content = Content.Remove(movePosition, extractLen);
            Content = Content.Insert(movePosition, newContent);
            startPosition += newContent.Length - extractLen;
        }
        public void Reset()
        {
            extractPosition = 0;
            movePosition = 0;
            extractedString = "";
            startPosition = 0;
        }

    }

    public static class Extract
    {
        public static string Between(string Src, string start1, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo_NotIncluding(start1);
            sp.exctractTo_NotIncluding(start2);
            return sp.extractedString;
        }

        public static string BetweenEnd(string Src, string start1)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo_NotIncluding(start1);
            sp.exctractToEnd2();
            return sp.extractedString;
        }

        public static string BetweenStart(string Src, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.exctractTo_NotIncluding(start2);
            return sp.extractedString;
        }


        public static string BetweenInclude(string Src, string start1, string start2)
        {
            StringsParser sp = new StringsParser(Src);
            sp.searchTo(start1);
            sp.exctractTo(start2);
            return sp.extractedString;
        }
    }
}
