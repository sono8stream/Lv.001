using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Data
{
    /// <summary>
    /// DataRecordの集約
    /// 保持するDataRecordの種類は問わない
    /// </summary>
    public class DataTable
    {
        public TableId Id { get; private set; }
        public Dictionary<RecordId, DataRecord> Records { get; private set; }

        public DataTable(TableId id, Dictionary<RecordId, DataRecord> records)
        {
            Id = id;
            Records = records;
        }
    }
}
