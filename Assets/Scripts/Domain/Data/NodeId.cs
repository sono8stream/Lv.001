using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    public class NodeId
    {
        private int value;

        public NodeId(int value)
        {
            this.value = value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as NodeId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value;
        }
    }
}
