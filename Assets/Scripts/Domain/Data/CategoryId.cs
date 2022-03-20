using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data


{
    public class CategoryId
    {
        private int value;

        public CategoryId(int value)
        {
            this.value = value;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CategoryId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value;
        }
    }
}
