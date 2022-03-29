using System;

namespace Domain.Data
{
    public class FieldId
    {
        private int value;

        public FieldId(int value)
        {
            this.value = value;
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

            return this.value == other.value;
        }
    }
}
