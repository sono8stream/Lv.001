using System;

namespace Domain.Data
{
    public class FieldId
    {
        public int Value { get; private set; }
        private string name;

        public FieldId(int value, string name)
        {
            this.Value = value;
            this.name = name;
        }

        public FieldId(int value)
        {
            this.Value = value;
            this.name = "";
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as FieldId;
            if (other == null)
            {
                return false;
            }

            return this.Value == other.Value;
        }
    }
}
