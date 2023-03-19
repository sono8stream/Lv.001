using System;
using System.Collections.Generic;
using Expression.Map.MapEvent;
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

        List<EventCommandBase[]> commandsList;

        public WolfCommonEventCommandsRepository()
        {
            commandsList = new List<EventCommandBase[]>();
            ReadCommonEvents();
        }

        public EventCommandBase[] GetCommands(int commonEventId)
        {
            if (0 <= commonEventId && commonEventId < commandsList.Count)
            {
                return commandsList[commonEventId];
            }

            return new EventCommandBase[0];
        }

        private void ReadCommonEvents()
        {
            var reader = new WolfDataReader(dirPath);
            int offset = 11;// オフセットスキップ
            int eventCount = reader.ReadInt(offset, true, out offset);

            for(int i = 0; i < eventCount; i++)
            {
                // イベントを読み出す
                //commandsList.Add(ReadCommonEvent(reader, ref offset));
            }
        }

        private EventCommandBase[] ReadCommonEvent(WolfDataReader reader, ref int offset)
        {
            reader.ReadByte(offset, out offset);// ヘッダーはスキップ
            int eventId = reader.ReadInt(offset, true, out offset);
            int conditionType = reader.ReadByte(offset, out offset);
            int conditionLeftValue = reader.ReadInt(offset, true, out offset);
            int conditionRightValue = reader.ReadInt(offset, true, out offset);

            int numberArgCount= reader.ReadByte(offset, out offset);
            int stringArgCount = reader.ReadByte(offset, out offset);

            string eventName = reader.ReadString(offset, out offset);
            int eventCommandLength = reader.ReadInt(offset, true, out offset);

            return null;
        }
    }
}
