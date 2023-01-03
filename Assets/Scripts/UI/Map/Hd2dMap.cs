using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression.Map;
using UI.Action;

namespace UI.Map
{
    public class Hd2dMap : MonoBehaviour
    {
        private int mapIndex;

        [SerializeField]
        private Shader spriteShader;

        [SerializeField]
        private GameObject eventObjectOrigin;

        private Hd2dMapData mapData;

        public List<EventObject> MapEvents { get; private set; }

        // Use this for initialization
        void Awake()
        {
            mapIndex = -1;

            var systemRepository = DI.DependencyInjector.It().SystemDataRepository;
            var dataRef = new Domain.Data.DataRef(new Domain.Data.TableId(7), new Domain.Data.RecordId(0), new Domain.Data.FieldId(0));
            int nextMapIndex = systemRepository.FindInt(dataRef).Val;
            nextMapIndex = 1;
            MapId nextMapId = new MapId(nextMapIndex);
            ChangeMap(nextMapId);
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public MovableInfo[,] GetMovableInfo()
        {
            if (mapData == null)
            {
                return null;
            }
            else
            {
                return mapData.MovableGrid;
            }
        }

        private void GenerateEventObjects(Hd2dMapData mapData)
        {
            MapEvents = new List<EventObject>();

            for (int i = 0; i < mapData.EventDataArray.Length; i++)
            {
                GameObject gameObject = Instantiate(eventObjectOrigin);
                Vector2Int pos = new Vector2Int(mapData.EventDataArray[i].PosX, mapData.EventDataArray[i].PosY);
                gameObject.transform.position = Util.Map.PositionConverter.GetUnityHd2dPos(pos, mapData.Height);

                EventObject eventObject = gameObject.GetComponent<EventObject>();
                if (eventObject)
                {
                    eventObject.SetEventData(mapData.EventDataArray[i], spriteShader);
                    MapEvents.Add(eventObject);
                }
            }
        }

        // マップを切り替える
        public void ChangeMap(MapId mapId)
        {
            if (mapId.Value == mapIndex)
            {
                return;
            }

            mapIndex = mapId.Value;

            if (MapEvents != null)
            {
                foreach (EventObject e in MapEvents)
                {
                    Destroy(e.gameObject);
                }
            }

            // マップ生成
            WolfHd2dMapFactory creator = new WolfHd2dMapFactory(mapId);
            mapData = creator.Create();

            GenerateEventObjects(mapData);
        }
    }
}
