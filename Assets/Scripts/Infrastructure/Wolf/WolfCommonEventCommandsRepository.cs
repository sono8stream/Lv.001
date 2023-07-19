using System;
using System.Collections.Generic;
using Expression.Map.MapEvent;
using Expression.Map.MapEvent.Command;
using Expression.Event;
using UnityEngine;
using Util.Wolf;

namespace Infrastructure
{
    /// <summary>
    /// Wolf用のコモンイベントを読み出す
    /// </summary>
    public class WolfCommonEventCommandsRepository : ICommonEventCommandsRepository
    {
        private string dirPath = $"{Application.streamingAssetsPath}/Data/BasicData/CommonEvent.dat";

        Dictionary<CommonEventId, CommonEvent> commandsDict;

        public WolfCommonEventCommandsRepository()
        {
            commandsDict = new Dictionary<CommonEventId, CommonEvent>();
            ReadCommonEvents();
        }

        public CommonEvent GetEvent(CommonEventId eventId)
        {
            if (commandsDict.ContainsKey(eventId))
            {
                return commandsDict[eventId];
            }

            return new CommonEvent(eventId, "無し", new EventCommandBase[0], null);
        }

        private void ReadCommonEvents()
        {
            var reader = new WolfDataReader(dirPath);
            int offset = 11;// オフセットスキップ
            int eventCount = reader.ReadInt(offset, true, out offset);
            WolfEventCommandFactory commandFactory = new WolfEventCommandFactory();

            for (int i = 0; i < eventCount; i++)
            {
                // イベントを読み出す
                var id = new CommonEventId(i);
                commandsDict.Add(id, ReadCommonEvent(reader, ref offset, commandFactory));
            }
        }

        private CommonEvent ReadCommonEvent(WolfDataReader reader, ref int offset,
            WolfEventCommandFactory commandFactory)
        {
            reader.ReadByte(offset, out offset);// ヘッダーはスキップ
            int eventIdRaw = reader.ReadInt(offset, true, out offset);
            CommonEventId eventId = new CommonEventId(eventIdRaw);

            int conditionType = reader.ReadByte(offset, out offset);
            int conditionLeftValue = reader.ReadInt(offset, true, out offset);
            int conditionRightValue = reader.ReadInt(offset, true, out offset);

            int numberArgCount = reader.ReadByte(offset, out offset);
            int stringArgCount = reader.ReadByte(offset, out offset);

            string eventName = reader.ReadString(offset, out offset);
            int eventCommandLength = reader.ReadInt(offset, true, out offset);
            EventCommandBase[] commands = new EventCommandBase[eventCommandLength];

            for (int i = 0; i < eventCommandLength; i++)
            {
                commands[i] = commandFactory.Create(reader, offset, out offset);
            }

            reader.ReadBytes(offset, 5, out offset);// スキップ

            string memo = reader.ReadString(offset, out offset);
            int versionInfo = reader.ReadByte(offset, out offset);

            // 引数名の数
            int argNameCount = reader.ReadInt(offset, true, out offset);
            argNameCount = 10;// V2もV3も10でないと正常動作しないらしい

            // 数値型引数名（V3以前前提）
            int numberArgMax = 5;
            int stringArgMax = argNameCount - numberArgMax;

            for (int i = 0; i < numberArgMax; i++)
            {
                string argName = reader.ReadString(offset, out offset);
            }

            for (int i = 0; i < stringArgMax; i++)
            {
                string argName = reader.ReadString(offset, out offset);
            }

            int argSpecifyTypeCount = reader.ReadInt(offset, true, out offset);
            // 数値引数の特殊指定
            int[] argSpecifyTypes = new int[argSpecifyTypeCount];
            for (int i = 0; i < argSpecifyTypeCount; i++)
            {
                argSpecifyTypes[i] = reader.ReadByte(offset, out offset);
            }

            // 数値特殊指定文字列パラメータ
            int stringArgSpecifyParamCount = reader.ReadInt(offset, true, out offset);
            for (int i = 0; i < stringArgSpecifyParamCount; i++)
            {
                int paramCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < paramCount; j++)
                {
                    reader.ReadString(offset, out offset);
                }
            }

            // 数値特殊指定数値パラメータ
            int numberArgSpecifyParamCount = reader.ReadInt(offset, true, out offset);
            for (int i = 0; i < numberArgSpecifyParamCount; i++)
            {
                int paramCount = reader.ReadInt(offset, true, out offset);
                for (int j = 0; j < paramCount; j++)
                {
                    reader.ReadInt(offset, true, out offset);
                }
            }
            reader.ReadBytes(offset, 20, out offset);

            // 引数初期値
            int argInitValueCount = reader.ReadInt(offset, true, out offset);
            int[] argInitValues = new int[argInitValueCount];
            for (int i = 0; i < argInitValueCount; i++)
            {
                argInitValues[i] = reader.ReadInt(offset, true, out offset);
            }

            int dummy = reader.ReadByte(offset, out offset);// 0x90が入っている想定

            int color = reader.ReadInt(offset, true, out offset);

            // セルフ変数名
            for (int i = 0; i < 100; i++)
            {
                reader.ReadString(offset, out offset);
            }

            dummy = reader.ReadByte(offset, out offset);// 0x91が入っている想定

            reader.ReadBytes(offset, 5, out offset);// スキップ

            dummy = reader.ReadByte(offset, out offset);// 0x92が入っている想定（V2以降）

            string retMeaning = reader.ReadString(offset, out offset);
            int retAddress = 15000000 + eventId.Value * 100 + reader.ReadInt(offset, true, out offset);
            var returnValueAccessorFactory = new WolfIntAccessorFactory(false, retAddress);

            dummy = reader.ReadByte(offset, out offset);

            var commonEvent = new CommonEvent(eventId, eventName, commands, returnValueAccessorFactory);
            return commonEvent;
        }

        public CommonEventId GetIdFromName(string name)
        {
            foreach(CommonEventId id in commandsDict.Keys)
            {
                if (commandsDict[id].Name == name)
                {
                    return id;
                }
            }

            return null;
        }

        public int GetCount()
        {
            return commandsDict.Count;
        }
    }
}
