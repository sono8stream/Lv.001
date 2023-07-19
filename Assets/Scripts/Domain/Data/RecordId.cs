using System;

namespace Domain.Data


{
    public class RecordId
    {
        public int Value { get; private set; }
        private string name;

        public RecordId(int value,string name)
        {
            this.Value = value;
            this.name = name;
        }

        public RecordId(int value)
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
            var other = obj as RecordId;
            if (other == null)
            {
                return false;
            }

            return this.Value == other.Value;
        }
    }
}
