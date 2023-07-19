using System;

using Expression.Common;

namespace Expression.Event
{
    public class CommonEvent : IEvent
    {
        public CommonEventId Id { get; private set; }

        public string Name { get; private set; }

        // yb’èzEventPageDataŠÜ‚ŞEventCommand‚ğEvent–¼‘O‹óŠÔ‚ÉˆÚ“®‚·‚é
        public Map.MapEvent.EventCommandBase[] EventCommands { get; private set; }

        public int[] NumberVariables { get; set; }

        public string[] StringVariables { get; set; }

        // –ß‚è’l‚Æ‚µ‚Ä•Ô‚·ƒZƒ‹ƒt•Ï”‚ÌID
        public IDataAccessorFactory<int> ReturnValueAccessorFactory { get; private set; }

        public bool HasReturnValue { get; private set; }

        public CommonEvent(CommonEventId id, string name, Map.MapEvent.EventCommandBase[] eventCommands,
            IDataAccessorFactory<int> returnValueAccessorFactory)
        {
            Id = id;
            Name = name;
            EventCommands = eventCommands;

            NumberVariables = new int[95];
            StringVariables = new string[5];

            HasReturnValue = returnValueAccessorFactory != null;
            ReturnValueAccessorFactory = returnValueAccessorFactory;
        }

        public void Visit(EventVisitorBase visitor)
        {
            visitor.OnVisitCommonEvent(this);
        }

        public void SetNumberArgs(int[] args)
        {
            int argCount = Math.Min(4, args.Length);
            for (int i = 0; i < argCount; i++)
            {
                NumberVariables[i] = args[i];
            }
        }
    }
}
