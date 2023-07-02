using System;

namespace Domain.Data


{
    public class RecordId
    {
        private int value;
        private string name;

        public RecordId(int value,string name)
        {
            this.value = value;
            this.name = name;
        }

        public RecordId(int value)
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
            var other = obj as RecordId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value;
        }
    }
}
