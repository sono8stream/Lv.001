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

        public RecordId(string name)
        {
            this.value = 0;
            this.name = name;
        }

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(name))
            {
                return value.GetHashCode();
            }
            else
            {
                return name.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as RecordId;
            if (other == null)
            {
                return false;
            }

            return this.value == other.value || (!string.IsNullOrEmpty(this.name) && this.name == other.name);
        }
    }
}
