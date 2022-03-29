using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Data;
using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG�̃Q�[���\���f�[�^��ǂݏo�����߂̃��|�W�g��
    /// �}�b�v�C�x���g�̃V�X�e���ϐ��A��l���̌��݈ʒu�Ȃ�
    /// </summary>
    public class WolfExpressionDataRepository : IExpressionDataRepository
    {
        private Dictionary<TableId, DataTable> mapVariableDict;

        public WolfExpressionDataRepository()
        {
            mapVariableDict = new Dictionary<TableId, DataTable>();
            IMapDataRepository mapDataRepository = DI.DependencyInjector.It().MapDataRepository;

            // �y�b��z�V�X�e���ϐ���o�^����܂Ń}�b�v����ID�͌��ߑł��Ƃ���
            for(int i = 0; i < 4; i++)
            {
                var mapId = new MapId(i);
                MapData mapData = mapDataRepository.Find(mapId);
                int eventCount = mapData.EventDataArray.Length;
                
                var mapVariableRecords = new Dictionary<RecordId, DataRecord>();

                for (int j = 0; j < eventCount; j++)
                {
                    // �}�b�v�C�x���g�̃Z���t�ϐ���10�̐����^
                    var intFields = new Dictionary<FieldId, DataField<int>>();
                    for (int k = 0; k < 10; k++)
                    {
                        var fieldId = new FieldId(k);
                        intFields.Add(fieldId, new DataField<int>(fieldId,0));
                    }
                    var stringFields = new Dictionary<FieldId, DataField<string>>();

                    var recordId = new RecordId(j);
                    DataRecord record = new DataRecord(recordId, intFields, stringFields);
                    mapVariableRecords.Add(recordId, record);
                }

                var tableId = new TableId(i);
                var mapVariableTable = new DataTable(tableId, mapVariableRecords);
                mapVariableDict.Add(tableId, mapVariableTable);
            }
        }

        public DataRecord Find(RecordId id)
        {
            return null;
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }
    }
}
