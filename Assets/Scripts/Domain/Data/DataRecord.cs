using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// 関連のあるDataFieldをひとまとめにするクラス
    /// </summary>
    public class DataRecord
    {
        public RecordId Id { get; private set; }

        public Dictionary<FieldId, DataField<int>> IntFields { get; private set; }

        public Dictionary<FieldId, DataField<string>> StringFields { get; private set; }

        public DataRecord(RecordId id, Dictionary<FieldId, DataField<int>> intFields, 
            Dictionary<FieldId, DataField<string>> stringFields)
        {
            Id = id;
            IntFields = intFields;
            StringFields = stringFields;
        }
    }
}
