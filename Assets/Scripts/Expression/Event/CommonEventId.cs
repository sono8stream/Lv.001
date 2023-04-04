using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expression.Event
{
    public class CommonEventId
    {
        public int Value { get; private set; }

        public CommonEventId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CommonEventId;
            if (other == null)
            {
                return false;
            }

            return this.Value == other.Value;
        }
    }
}
