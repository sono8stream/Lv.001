using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class EventId
    {
        public int Value { get; private set; }

        public EventId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as EventId;
            if (other == null)
            {
                return false;
            }

            return this.Value == other.Value;
        }
    }
}
