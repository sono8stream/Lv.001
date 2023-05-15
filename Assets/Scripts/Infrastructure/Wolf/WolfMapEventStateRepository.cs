using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Data;
using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのマップイベントの変数を保持します
    /// </summary>
    public class WolfMapEventStateRepository : IExpressionDataRepository
    {
        private Dictionary<TableId, DataTable> variableDict;

        public WolfMapEventStateRepository()
        {
            variableDict = new Dictionary<TableId, DataTable>();
            IMapDataRepository mapDataRepository = DI.DependencyInjector.It().MapDataRepository;

            for (int i = 0; i < mapDataRepository.GetCount(); i++)
            {
                var mapId = new MapId(i);
                MapData mapData = mapDataRepository.Find(mapId);
                int eventCount = mapData.EventDataArray.Length;

                var mapVariableRecords = new Dictionary<RecordId, DataRecord>();

                for (int j = 0; j < eventCount; j++)
                {
                    // マップイベントのセルフ変数は10個の整数型
                    var intFields = new Dictionary<FieldId, DataField<int>>();
                    for (int k = 0; k < 10; k++)
                    {
                        var fieldId = new FieldId(k);
                        intFields.Add(fieldId, new DataField<int>(fieldId, 0));
                    }
                    var stringFields = new Dictionary<FieldId, DataField<string>>();

                    var recordId = new RecordId(j);
                    DataRecord record = new DataRecord(recordId, intFields, stringFields);
                    mapVariableRecords.Add(recordId, record);
                }

                var tableId = new TableId(i);
                var mapVariableTable = new DataTable(tableId, mapVariableRecords);
                variableDict.Add(tableId, mapVariableTable);
            }
        }

        public DataRecord Find(RecordId id)
        {
            return null;
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            DataTable table = variableDict[dataRef.TableId];
            DataRecord record = table.Records[dataRef.RecordId];
            return record.IntFields[dataRef.FieldId];
        }

        public void SetInt(DataRef dataRef, int value)
        {
            DataTable table = variableDict[dataRef.TableId];
            DataRecord record = table.Records[dataRef.RecordId];
            record.IntFields[dataRef.FieldId].Val = value;
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            DataTable table = variableDict[dataRef.TableId];
            DataRecord record = table.Records[dataRef.RecordId];
            return new DataField<string>(dataRef.FieldId,
                record.IntFields[dataRef.FieldId].Val.ToString());
        }

        public void SetString(DataRef dataRef, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
