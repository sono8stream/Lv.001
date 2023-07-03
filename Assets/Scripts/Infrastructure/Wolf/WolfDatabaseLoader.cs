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
            // Wolfで管理するモデルに適した形式でデータを読み取り、その後オンメモリデータベースを構築
            LoadDatabaseRaw(dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records);
            CreateDatabase(schemas, records, out intDict, out strDict);
        }

        public void LoadDatabaseRaw(WolfConfig.DatabaseType dbType,
            out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records)
        {
            // .projectファイルと.datファイルを用いてオンメモリデータストアを構築
            LoadTypes(dbType, out schemas, out records);
            LoadDataAll(dbType, ref schemas, ref records);
        }

        /// <summary>
        /// DataBase.projectファイルからDBタイプを読み出す
        /// </summary>
        /// <param name="projPath">.projectファイルパス</param>
        /// <returns></returns>
        private void LoadTypes(WolfConfig.DatabaseType dbType, out WolfDatabaseSchema[] schemas, out WolfDatabaseRecord[][] records)
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
                records[i] = new WolfDatabaseRecord(dataNames[i]);
            }

            nextOffset = offset;
            schema = new WolfDatabaseSchema(name, columns);
        }

        private void LoadDataAll(WolfConfig.DatabaseType dbType,
            ref WolfDatabaseSchema[] schemas, ref WolfDatabaseRecord[][] records)
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
            WolfDatabaseRecord[] prevRecords = null;
            for (int i = 0; i < typeCount; i++)
            {
                LoadData(reader, offset, out offset, ref schemas[i], ref records[i], out bool isUsePrevName, prevRecords);
                
                // タイプ名を複数回引き継ぐ場合、最も項目が多い（引継ぎ元の）スキーマを引き継ぐようにする必要がある。
                // 例えば、0番目のスキーマを1、2番目で継承する場合にデータ数がそれぞれ10,1,5とする。
                // このとき、1番目よりも2番目のスキーマの方がデータ数が多い場合、0番目のスキーマの名前を取って5番目のデータまで名前が割り当てられる。
                if (!isUsePrevName)
                {
                    prevRecords = records[i];
                }
            }
        }

        private void LoadData(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset,
            ref WolfDatabaseSchema schema, ref WolfDatabaseRecord[] records, out bool isUsePrevDataName,
            WolfDatabaseRecord[] prevRecords)
        {
            reader.ReadInt(offset, true, out offset);// ヘッダの読み取り
            int idSelectType = reader.ReadInt(offset, true, out offset);// データ名を指定する方法。

            int columnCount = reader.ReadInt(offset, true, out offset);
            int[] columnTypes = new int[columnCount];// 数値か文字列かを保持
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

            isUsePrevDataName = false;
            if (idSelectType == 0x01)
            {
                // Field内の最初の文字列データを取得してレコード名とする
                if (stringCount > 0)
                {
                    for (int i = 0; i < dataCount; i++)
                    {
                        records[i].Name = records[i].StringData[0];
                    }
                }
            }
            else if (idSelectType == 0x02)
            {
                // 1つ前のタイプのデータIDを割り当てる
                if (prevRecords != null)
                {
                    int iterCount = Mathf.Min(records.Length, prevRecords.Length);
                    for (int i = 0; i < iterCount; i++)
                    {
                        records[i].Name = prevRecords[i].Name;
                    }
                }
                isUsePrevDataName = true;
            }

            nextOffset = offset;
        }

        /// <summary>
        /// 読み込んだWolfDatabase構造からオンメモリデータベースを生成
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
