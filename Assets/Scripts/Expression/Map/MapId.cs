using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class MapId
    {
        public int Value { get; private set; }

        public MapId(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as MapId;
            if (other == null)
            {
                return false;
            }

            return this.Value == other.Value;
        }
    }
}
