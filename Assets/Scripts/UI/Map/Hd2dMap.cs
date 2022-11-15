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

        private Expression.Map.Hd2dMapData mapData;

        // Use this for initialization
        void Awake()
        {
            var systemRepository = DI.DependencyInjector.It().SystemDataRepository;
            var dataRef = new Domain.Data.DataRef(new Domain.Data.TableId(7), new Domain.Data.RecordId(0), new Domain.Data.FieldId(0));
            mapIndex = systemRepository.FindInt(dataRef).Val;
            Expression.Map.MapId mapId = new Expression.Map.MapId(mapIndex);

            WolfHd2dMapFactory creator = new WolfHd2dMapFactory(mapId);
            mapData = creator.Create();

            GenerateEventObjects(mapData);
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public Expression.Map.MovableInfo[,] GetMovableInfo()
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

        private void GenerateEventObjects(Expression.Map.Hd2dMapData mapData)
        {
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
    }
}
