using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfShowTextCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            string text = metaCommand.StringArgs[0];
            var factories = GetFactories(text);
            return new MessageCommand(factories);
        }

        private List<Common.IDataAccessorFactory<string>> GetFactories(string text)
        {
            var factories = new List<Common.IDataAccessorFactory<string>>();
            string constStr = "";

            // ì¡éÍï∂éöÇéÊÇËèoÇ∑
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

                    // ì¡éÍï∂éöÇÃèàóùÇçsÇ§
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
    }
}