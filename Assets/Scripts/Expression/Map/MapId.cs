using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class MapId
    {
        private int value;

        public MapId(int value)
        {
            this.value = value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as MapId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value;
        }
    }
}
