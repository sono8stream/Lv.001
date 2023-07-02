using System;

namespace Domain.Data
{
    public class TableId
    {
        private int value;
        private string name;

        public TableId(int value, string name)
        {
            this.value = value;
            this.name = name;
        }

        public TableId(int value)
        {
            this.value = value;
            this.name = "";
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as TableId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value;
        }
    }
}
