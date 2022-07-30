using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression.Map;

namespace UI.Map
{
    public class Hd2dMap : MonoBehaviour
    {
        [SerializeField]
        private int mapIndex;

        [SerializeField]
        private Shader spriteShader;

        [SerializeField]
        private GameObject eventObjectOrigin;

        private Expression.Map.Hd2dMapData mapData;

        // Use this for initialization
        void Awake()
        {
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
                Vector2 unityPos = Util.Map.PositionConverter.GetUnityPos(pos, mapData.Height);
                gameObject.transform.position = new Vector3(unityPos.x, 1, unityPos.y);

                ActionProcessor eventObject = gameObject.GetComponent<ActionProcessor>();
                if (eventObject)
                {
                    eventObject.SetEventData(mapData.EventDataArray[i]);
                }

                // この処理はカプセル化できるのでEventObjectクラスに委譲したほうがよさそう
                Texture2D currentTexture = mapData.EventDataArray[i].PageData[0].GetCurrentTexture();

                if (currentTexture == null)
                {
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    Material mat = new Material(spriteShader);
                    mat.mainTexture = currentTexture;
                    mat.mainTexture.filterMode = FilterMode.Point;
                    gameObject.GetComponent<Renderer>().sharedMaterial = mat;
                }
            }
        }
    }
}
