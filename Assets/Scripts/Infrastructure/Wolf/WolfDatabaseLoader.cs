using Domain.Data;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのDBファイルを読み込むためのローダー
    /// </summary>
    public class WolfDatabaseLoader
    {
        public void LoadDatabase(WolfConfig.DatabaseType dbType, out Dictionary<DataRef, int> intDict, out Dictionary<DataRef, string> strDict)
        {
            // .projectファイルと.datファイルを用いてオンメモリデータストアを構築
            LoadTypes(dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records);
            LoadDataAll(dbType, schemas, records, out intDict, out strDict);
        }

        /// <summary>
        /// DataBase.projectファイルからDBタイプを読み出す
        /// </summary>
        /// <param name="projPath">.projectファイルパス</param>
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
        /// 指定されたreaderを用いて単一のDBタイプを読み出す
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

            reader.ReadString(offset, out offset);// メモの読み取り
            int customSelectCount = reader.ReadInt(offset, true, out offset);// 0x64固定。特殊な選択ボックスの数
            int[] customSelectTypes = new int[customSelectCount];
            for (int i = 0; i < customSelectCount; i++)
            {
                customSelectTypes[i] = reader.ReadByte(offset, out offset);
            }

            int memoCount = reader.ReadInt(offset, true, out offset);// 項目数に一致。項目メモの数
            for (int i = 0; i < columnCount; i++)
            {
                // 各項目のメモの読み取り。エディタからは見えないので無視
                reader.ReadString(offset, out offset);
            }

            int stringParams = reader.ReadInt(offset, true, out offset);// 項目数に一致。文字列データの特殊設定数を取得
            for (int i = 0; i < stringParams; i++)
            {
                // 選択可能な項目の数を示す
                int stringSelectCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < stringSelectCount; j++)
                {
                    reader.ReadString(offset, out offset);// 取り出しても意味がないので無視
                }
            }

            int numberParams = reader.ReadInt(offset, true, out offset);// 項目数に一致。数値データの特殊設定数を取得
            for (int i = 0; i < numberParams; i++)
            {
                // 選択可能な項目の数を示す
                int intSelectCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < intSelectCount; j++)
                {
                    reader.ReadInt(offset, true, out offset);// 取り出しても意味がないので無視
                }
            }

            int initValueCount = reader.ReadInt(offset, true, out offset);// 項目数に一致。初期値の数を取得
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
            // ヘッダーをスキップ
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
            reader.ReadInt(offset, true, out offset);// ヘッダの読み取り
            int idSelectType = reader.ReadInt(offset, true, out offset);// データ名を指定する方法。
            int columnCount = reader.ReadInt(offset, true, out offset);
            int[] columnTypes = new int[columnCount];// 数値か文字列かを保持
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
