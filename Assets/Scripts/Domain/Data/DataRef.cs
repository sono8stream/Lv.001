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
    }
}
