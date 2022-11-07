using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expression.Map.MapEvent
{
    class MetaEventCommand
    {
        public int[] NumberArgs { get; private set; }

        public string[] StringArgs { get; private set; }

        public int IndentDepth { get; private set; }

        public int FooterValue { get; private set; }

        public MetaEventCommand(int[] numberArgs, string[] stringArgs,int indentDepth,int footerValue)
        {
            NumberArgs = numberArgs;
            StringArgs = stringArgs;
            IndentDepth = indentDepth;
            FooterValue = footerValue;
        }
    }
}
