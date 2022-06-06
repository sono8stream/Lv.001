using System;

namespace Domain.Data


{
    public class RecordId
    {
        private int value;

        public RecordId(int value)
        {
            this.value = value;
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
