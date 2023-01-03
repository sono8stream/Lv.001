using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;
using UI.Action;

namespace UI.Map
{
    /// <summary>
    /// �y�b��zHd2dMap�ɒu�������\�B�d�����������邪�A���ʉ�����Ă��Ȃ����ߗ��p�s�\�B�g�p���Ȃ��悤����
    /// </summary>
    public class Map : MonoBehaviour
    {
        [SerializeField]
        private int mapIndex;

        [SerializeField]
        private GameObject eventObjectOrigin;

        private Expression.Map.MapData mapData;

        // Use this for initialization
        void Awake()
        {
            Expression.Map.MapId mapId = new Expression.Map.MapId(mapIndex);
            mapData = DI.DependencyInjector.It().MapDataRepository.Find(mapId);

            SpriteRenderer underRenderer = transform.Find("UnderSprite").GetComponent<SpriteRenderer>();

            Sprite underSprite = Sprite.Create(mapData.UnderTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
            underSprite.texture.filterMode = FilterMode.Point;
            underRenderer.sprite = underSprite;

            SpriteRenderer upperRenderer = transform.Find("UpperSprite").GetComponent<SpriteRenderer>();

            Sprite upperSprite = Sprite.Create(mapData.UpperTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
            upperSprite.texture.filterMode = FilterMode.Point;
            upperRenderer.sprite = upperSprite;

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

        private void GenerateEventObjects(Expression.Map.MapData mapData)
        {
            for (int i = 0; i < mapData.EventDataArray.Length; i++)
            {
                GameObject gameObject = Instantiate(eventObjectOrigin);
                Vector2Int pos = new Vector2Int(mapData.EventDataArray[i].PosX, mapData.EventDataArray[i].PosY);
                gameObject.transform.position = Util.Map.PositionConverter.GetUnityPos(pos, mapData.Height);

                ActionProcessor eventObject = gameObject.GetComponent<ActionProcessor>();
                if (eventObject)
                {
                    eventObject.SetEventData(mapData.EventDataArray[i], null);
                }
            }
        }

        // �}�b�v��؂�ւ���
        public void ChangeMap(Expression.Map.MapId mapId)
        {
            if (mapId.Value == mapIndex)
            {
                return;
            }

            mapIndex = mapId.Value;
            mapData = DI.DependencyInjector.It().MapDataRepository.Find(mapId);

            // �}�b�v���N���A



            // �}�b�v����
            SpriteRenderer underRenderer = transform.Find("UnderSprite").GetComponent<SpriteRenderer>();

            Sprite underSprite = Sprite.Create(mapData.UnderTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
            underSprite.texture.filterMode = FilterMode.Point;
            underRenderer.sprite = underSprite;

            SpriteRenderer upperRenderer = transform.Find("UpperSprite").GetComponent<SpriteRenderer>();

            Sprite upperSprite = Sprite.Create(mapData.UpperTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
            upperSprite.texture.filterMode = FilterMode.Point;
            upperRenderer.sprite = upperSprite;

            GenerateEventObjects(mapData);
        }
    }
}
