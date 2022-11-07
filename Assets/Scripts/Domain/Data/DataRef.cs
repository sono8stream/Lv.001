using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// 特定のデータにアクセスするための情報を保持するクラス
    /// </summary>
    public class DataRef
    {
        public TableId TableId { get; private set; }
        public RecordId RecordId { get; private set; }
        public FieldId FieldId { get; private set; }

        public DataRef(TableId tableId, RecordId recordId, FieldId fieldId)
        {
            TableId = tableId;
            RecordId = recordId;
            FieldId = fieldId;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DataRef;
            if (other == null)
            {
                return false;
            }

            return this.TableId == other.TableId
                && this.RecordId == other.RecordId
                && this.FieldId == other.FieldId;
        }

        public override int GetHashCode()
        {
            return TableId.GetHashCode() ^ RecordId.GetHashCode() ^ FieldId.GetHashCode();
        }
    }
}
