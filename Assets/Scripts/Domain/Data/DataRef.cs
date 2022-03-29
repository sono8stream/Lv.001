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
        private TableId tableId;
        private RecordId recordId;
        private FieldId fieldId;

        public DataRef(TableId tableId, RecordId recordId, FieldId fieldId)
        {
            this.recordId = recordId;
            this.fieldId = fieldId;
        }
    }
}
