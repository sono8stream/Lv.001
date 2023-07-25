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
            IStringFactory factory = new WolfStringFactory(text);
            return new Command.MessageCommand(metaCommand.IndentDepth, factory);
        }
    }
}