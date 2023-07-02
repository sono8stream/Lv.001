using System;

namespace Domain.Data
{
    public class FieldId
    {
        private int value;
        private string name;

        public FieldId(int value, string name)
        {
            this.value = value;
            this.name = name;
        }

        public FieldId(int value)
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
            var other = obj as FieldId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value || (!string.IsNullOrEmpty(this.name) && this.name == other.name);
        }
    }
}
