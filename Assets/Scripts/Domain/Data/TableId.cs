﻿using System;

namespace Domain.Data
{
    public class TableId
    {
        private int value;

        public TableId(int value)
        {
            this.value = value;
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