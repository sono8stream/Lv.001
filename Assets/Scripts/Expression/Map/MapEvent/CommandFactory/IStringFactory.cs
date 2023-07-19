using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public interface IStringFactory
    {
        public string GenerateMessage(CommandVisitContext context);
    }
}