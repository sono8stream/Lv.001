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
            // Wolf�ŊǗ����郂�f���ɓK�����`���Ńf�[�^��ǂݎ��A���̌�I���������f�[�^�x�[�X���\�z
            LoadDatabaseRaw(dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records);
            CreateDatabase(schemas, records, out intDict, out strDict);
        }

        public void LoadDatabaseRaw(WolfConfig.DatabaseType dbType,
            out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records)
        {
            // .project�t�@�C����.dat�t�@�C����p���ăI���������f�[�^�X�g�A���\�z
            LoadTypes(dbType, out schemas, out records);
            LoadDataAll(dbType, ref schemas, ref records);
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
                records[i] = new WolfDatabaseRecord(dataNames[i]);
            }

            nextOffset = offset;
            schema = new WolfDatabaseSchema(name, columns);
        }

        public void LoadDataAll(WolfConfig.DatabaseType dbType,
            ref WolfDatabaseSchema[] schemas, ref WolfDatabaseRecord[][] records)
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
            for (int i = 0; i < typeCount; i++)
            {
                LoadData(reader, offset, out offset, ref schemas[i], ref records[i]);
            }
        }

        private void LoadData(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset,
            ref WolfDatabaseSchema schema, ref WolfDatabaseRecord[] records)
        {
            reader.ReadInt(offset, true, out offset);// �w�b�_�̓ǂݎ��
            int idSelectType = reader.ReadInt(offset, true, out offset);// �f�[�^�����w�肷����@�B
            int columnCount = reader.ReadInt(offset, true, out offset);
            int[] columnTypes = new int[columnCount];// ���l�������񂩂�ێ�
            int numberCount = 0;
            int stringCount = 0;

            for (int i = 0; i < columnCount; i++)
            {
                columnTypes[i] = reader.ReadInt(offset, true, out offset);
                if (columnTypes[i] < 2000)
                {
                    numberCount++;
                    schema.Columns[i].Type = WolfDatabaseColumn.ColumnType.Int;
                }
                else
                {
                    stringCount++;
                    schema.Columns[i].Type = WolfDatabaseColumn.ColumnType.String;
                }
            }

            int dataCount = reader.ReadInt(offset, true, out offset);
            for (int i = 0; i < dataCount; i++)
            {
                records[i].IntData = new int[numberCount];
                for (int j = 0; j < numberCount; j++)
                {
                    int value = reader.ReadInt(offset, true, out offset);
                    records[i].IntData[j] = value;
                }

                records[i].StringData = new string[stringCount];
                for (int j = 0; j < stringCount; j++)
                {
                    string value = reader.ReadString(offset, out offset);
                    records[i].StringData[j] = value;
                }
            }

            nextOffset = offset;
        }

        /// <summary>
        /// �ǂݍ���WolfDatabase�\������I���������f�[�^�x�[�X�𐶐�
        /// </summary>
        /// <param name="schemas"></param>
        /// <param name="records"></param>
        /// <param name="intDict"></param>
        /// <param name="strDict"></param>
        private void CreateDatabase(WolfDatabaseSchema[] schemas, WolfDatabaseRecord[][] records,
            out Dictionary<DataRef, int> intDict,
            out Dictionary<DataRef, string> strDict)
        {
            intDict = new Dictionary<DataRef, int>();
            strDict = new Dictionary<DataRef, string>();
            for (int i = 0; i < schemas.Length; i++)
            {
                TableId tableId = new TableId(i, schemas[i].Name);

                for (int j = 0; j < records[i].Length; j++)
                {
                    RecordId recordId = new RecordId(j, records[i][j].Name);

                    int intI = 0;
                    int strI = 0;
                    for(int k = 0; k < schemas[i].Columns.Length; k++)
                    {
                        FieldId fieldId = new FieldId(k);
                        DataRef dataRef = new DataRef(tableId, recordId, fieldId);

                        if (schemas[i].Columns[k].Type == WolfDatabaseColumn.ColumnType.Int)
                        {
                            intDict.Add(dataRef, records[i][j].IntData[intI]);
                            intI++;
                        }
                        else
                        {
                            strDict.Add(dataRef, records[i][j].StringData[strI]);
                            strI++;
                        }
                    }
                }
            }
        }
    }
}
