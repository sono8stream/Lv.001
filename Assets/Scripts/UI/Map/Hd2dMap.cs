using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;

namespace UI.Map
{
    public class Hd2dMap : MonoBehaviour
    {
        [SerializeField]
        private int mapIndex;

        [SerializeField]
        private GameObject eventObjectOrigin;

        private Expression.Map.Hd2dMapData mapData;

        // Use this for initialization
        void Awake()
        {
            Expression.Map.MapId mapId = new Expression.Map.MapId(mapIndex);
            mapData = DI.DependencyInjector.It().Hd2dMapDataRepository.Find(mapId);

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
                gameObject.transform.position = Util.Map.PositionConverter.GetUnityPos(pos, mapData.Height);

                ActionProcessor eventObject = gameObject.GetComponent<ActionProcessor>();
                if (eventObject)
                {
                    eventObject.SetEventData(mapData.EventDataArray[i]);
                }

                // この処理はカプセル化できるのでEventObjectクラスに委譲したほうがよさそう
                Texture2D currentTexture = mapData.EventDataArray[i].PageData[0].GetCurrentTexture();
                SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
                if (currentTexture == null)
                {
                    spriteRenderer.sprite = null;
                }
                else
                {
                    // 【暫定】ピクセルサイズをDB層から取り出す
                    int pixelPerUnit = 16;
                    Sprite sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f), pixelPerUnit);
                    sprite.texture.filterMode = FilterMode.Point;
                    spriteRenderer.sprite = sprite;
                }
            }
        }
    }
}
