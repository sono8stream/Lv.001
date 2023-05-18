using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class StringFactory
    {
        private List<Common.IDataAccessorFactory<string>> factories;

        public StringFactory(string text)
        {
            factories = CreateFactories(text);
        }

        List<Common.IDataAccessorFactory<string>> CreateFactories(string text)
        {
            var factories = new List<Common.IDataAccessorFactory<string>>();
            string constStr = "";

            // “Áê•¶š‚ğæ‚èo‚·
            // yb’èzƒZƒ‹ƒt•Ï”ŒÄ‚Ño‚µ‚É‚µ‚©‘Î‰‚µ‚Ä‚¢‚È‚¢‚Ì‚ÅAC³
            var matches = Regex.Matches(text, @"\\c?self\[[0-9]+\]");
            int matchIndex = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (matchIndex < matches.Count && matches[matchIndex].Index == i)
                {
                    if (constStr.Length > 0)
                    {
                        factories.Add(
                            new Command.WolfStringAccessorFactory(true, constStr));
                        constStr = "";
                    }

                    // “Áê•¶š‚Ìˆ—‚ğs‚¤
                    factories.Add(new Command.WolfStringAccessorFactory(
                        false, matches[matchIndex].Value));
                    i += matches[matchIndex].Value.Length - 1;
                    matchIndex++;
                }
                else
                {
                    constStr += text[i];
                }
            }
            if (constStr.Length > 0)
            {
                factories.Add(new Command.WolfStringAccessorFactory(
                    true, constStr));
            }
            return factories;
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            string message = "";
            for (int i = 0; i < factories.Count; i++)
            {
                message += factories[i].Create(context).Get();
            }

            return message;
        }
    }
}