using Domain.Data;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG��DB�t�@�C����ǂݍ��ނ��߂̃��[�_�[
    /// </summary>
    public class WolfDatabaseLoader
    {
        public void LoadDatabase(WolfConfig.DatabaseType dbType, out Dictionary<DataRef, int> intDict, out Dictionary<DataRef, string> strDict)
        {
            // .project�t�@�C����.dat�t�@�C����p���ăI���������f�[�^�X�g�A���\�z
            LoadTypes(dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records);
            LoadDataAll(dbType, schemas, records, out intDict, out strDict);
        }

        /// <summary>
        /// DataBase.project�t�@�C������DB�^�C�v��ǂݏo��
        /// </summary>
        /// <param name="projPath">.project�t�@�C���p�X</param>
        /// <returns></returns>
        public void LoadTypes(WolfConfig.DatabaseType dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records)
        {
            string projPath = WolfConfig.GetDbProjectPath(dbType);
            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(projPath);
            int offset = 0;
            int columns = reader.ReadInt(offset, true, out offset);
            schemas = new WolfDatabaseSchema[columns];
            records = new WolfDatabaseRecord[columns][];

            for (int i = 0; i < columns; i++)
            {
                LoadType(reader, offset, out offset, ref schemas[i], ref records[i]);
            }
        }

        /// <summary>
        /// �w�肳�ꂽreader��p���ĒP���DB�^�C�v��ǂݏo��
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private void LoadType(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset,
            ref WolfDatabaseSchema schema, ref WolfDatabaseRecord[] records)
        {
            string name = reader.ReadString(offset, out offset);
            int columnCount = reader.ReadInt(offset, true, out offset);

            string[] columnNames = new string[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                columnNames[i] = reader.ReadString(offset, out offset);
            }

            int dataCount = reader.ReadInt(offset, true, out offset);

            string[] dataNames = new string[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                dataNames[i] = reader.ReadString(offset, out offset);
            }

            reader.ReadString(offset, out offset);// �����̓ǂݎ��
            int customSelectCount = reader.ReadInt(offset, true, out offset);// 0x64�Œ�B����ȑI���{�b�N�X�̐�
            int[] customSelectTypes = new int[customSelectCount];
            for (int i = 0; i < customSelectCount; i++)
            {
                customSelectTypes[i] = reader.ReadByte(offset, out offset);
            }

            int memoCount = reader.ReadInt(offset, true, out offset);// ���ڐ��Ɉ�v�B���ڃ����̐�
            for (int i = 0; i < columnCount; i++)
            {
                // �e���ڂ̃����̓ǂݎ��B�G�f�B�^����͌����Ȃ��̂Ŗ���
                reader.ReadString(offset, out offset);
            }

            int stringParams = reader.ReadInt(offset, true, out offset);// ���ڐ��Ɉ�v�B������f�[�^�̓���ݒ萔���擾
            for (int i = 0; i < stringParams; i++)
            {
                // �I���\�ȍ��ڂ̐�������
                int stringSelectCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < stringSelectCount; j++)
                {
                    reader.ReadString(offset, out offset);// ���o���Ă��Ӗ����Ȃ��̂Ŗ���
                }
            }

            int numberParams = reader.ReadInt(offset, true, out offset);// ���ڐ��Ɉ�v�B���l�f�[�^�̓���ݒ萔���擾
            for (int i = 0; i < numberParams; i++)
            {
                // �I���\�ȍ��ڂ̐�������
                int intSelectCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < intSelectCount; j++)
                {
                    reader.ReadInt(offset, true, out offset);// ���o���Ă��Ӗ����Ȃ��̂Ŗ���
                }
            }

            int initValueCount = reader.ReadInt(offset, true, out offset);// ���ڐ��Ɉ�v�B�����l�̐����擾
            int[] initValues = new int[initValueCount];
            for (int i = 0; i < initValueCount; i++)
            {
                initValues[i] = reader.ReadInt(offset, true, out offset);
            }

            WolfDatabaseColumn[] columns = new WolfDatabaseColumn[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                columns[i] = new WolfDatabaseColumn(columnNames[i], WolfDatabaseColumn.ColumnType.Int, initValues[i]);
            }

            records = new WolfDatabaseRecord[dataCount];
            for (int i = 0; i < dataCount; i++)
            {
                records[i] = new WolfDatabaseRecord(dataNames[i], null, null);
            }

            nextOffset = offset;
            schema = new WolfDatabaseSchema(name, columns);
        }

        public void LoadDataAll(WolfConfig.DatabaseType dbType,
            WolfDatabaseSchema[] schemas, WolfDatabaseRecord[][] records,
            out Dictionary<DataRef, int> intDict,
            out Dictionary<DataRef, string> strDict)
        {
            string datPath = WolfConfig.GetDbDatPath(dbType);
            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(datPath);
            int offset = 0;
            // �w�b�_�[���X�L�b�v
            for (int i = 0; i < 11; i++)
            {
                reader.ReadByte(offset, out offset);
            }

            int typeCount = reader.ReadInt(offset, true, out offset);
            intDict = new Dictionary<DataRef, int>();
            strDict = new Dictionary<DataRef, string>();
            for (int i = 0; i < typeCount; i++)
            {
                TableId tableId = new TableId(i, schemas[i].Name);
                LoadData(reader, offset, out offset, tableId, schemas[i], records[i], ref intDict, ref strDict);
            }
        }

        private void LoadData(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset,
            TableId tableId, WolfDatabaseSchema schema, WolfDatabaseRecord[] records,
            ref Dictionary<DataRef, int> intDict,
            ref Dictionary<DataRef, string> strDict)
        {
            reader.ReadInt(offset, true, out offset);// �w�b�_�̓ǂݎ��
            int idSelectType = reader.ReadInt(offset, true, out offset);// �f�[�^�����w�肷����@�B
            int columnCount = reader.ReadInt(offset, true, out offset);
            int[] columnTypes = new int[columnCount];// ���l�������񂩂�ێ�
            int numberCount = 0;
            List<FieldId> numbers = new List<FieldId>();
            int stringCount = 0;
            List<FieldId> strings = new List<FieldId>();
            for (int i = 0; i < columnCount; i++)
            {
                columnTypes[i] = reader.ReadInt(offset, true, out offset);
                if (columnTypes[i] < 2000)
                {
                    numberCount++;
                    numbers.Add(new FieldId(i, schema.Columns[i].Name));
                }
                else
                {
                    stringCount++;
                    strings.Add(new FieldId(i, schema.Columns[i].Name));
                }
            }

            int dataCount = reader.ReadInt(offset, true, out offset);
            for (int i = 0; i < dataCount; i++)
            {
                RecordId recordId = new RecordId(i, records[i].Name);
                for (int j = 0; j < numberCount; j++)
                {
                    DataRef dataRef = new DataRef(tableId, recordId, numbers[j]);
                    int value = reader.ReadInt(offset, true, out offset);
                    intDict.Add(dataRef, value);
                }

                for (int j = 0; j < stringCount; j++)
                {
                    DataRef dataRef = new DataRef(tableId, recordId, strings[j]);
                    string value = reader.ReadString(offset, out offset);
                    strDict.Add(dataRef, value);
                }
            }

            nextOffset = offset;
        }
    }
}
