using System;

namespace Domain.Data
{
    public class TableId
    {
        // 【暫定】数値をキーにした呼び出しを廃止し、文字列でのみ呼び出し可能とする
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

            return this.value == other.value || (!string.IsNullOrEmpty(this.name) && this.name == other.name);
        }
    }
}
