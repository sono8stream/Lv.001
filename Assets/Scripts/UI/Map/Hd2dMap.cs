using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression.Map;

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

        public List<ActionProcessor> MapEvents { get; private set; }

        // Use this for initialization
        void Awake()
        {
            mapIndex = -1;

            var systemRepository = DI.DependencyInjector.It().SystemDataRepository;
            var dataRef = new Domain.Data.DataRef(new Domain.Data.TableId(7), new Domain.Data.RecordId(0), new Domain.Data.FieldId(0));
            int nextMapIndex = systemRepository.FindInt(dataRef).Val;
            //nextMapIndex = 3;
            MapId nextMapId = new MapId(nextMapIndex);
            ChangeMap(nextMapId, null);
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
            MapEvents = new List<ActionProcessor>();

            for (int i = 0; i < mapData.EventDataArray.Length; i++)
            {
                GameObject gameObject = Instantiate(eventObjectOrigin);
                Vector2Int pos = new Vector2Int(mapData.EventDataArray[i].PosX, mapData.EventDataArray[i].PosY);
                gameObject.transform.position = Util.Map.PositionConverter.GetUnityHd2dPos(pos, mapData.Height);

                ActionProcessor eventObject = gameObject.GetComponent<ActionProcessor>();
                if (eventObject)
                {
                    eventObject.SetEventData(mapData.EventDataArray[i]);
                }
                MapEvents.Add(eventObject);

                // この処理はカプセル化できるのでEventObjectクラスに委譲したほうがよさそう
                Texture2D currentTexture = mapData.EventDataArray[i].PageData[0].GetCurrentTexture();

                if (currentTexture == null)
                {
                    gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
                }
                else
                {
                    Material mat = new Material(spriteShader);
                    mat.mainTexture = currentTexture;
                    mat.mainTexture.filterMode = FilterMode.Point;
                    gameObject.GetComponentInChildren<Renderer>().sharedMaterial = mat;
                }
            }
        }

        // マップを切り替える
        public void ChangeMap(MapId mapId, ActionProcessor calledEvent)
        {
            if (mapId.Value == mapIndex)
            {
                return;
            }

            mapIndex = mapId.Value;

            if (MapEvents != null)
            {
                foreach (ActionProcessor e in MapEvents)
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
