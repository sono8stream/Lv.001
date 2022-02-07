using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;

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
        mapData = Expression.WolfDependencyInjector.It().MapDataRepository.Find(mapId);

        SpriteRenderer underRenderer = transform.Find("UnderSprite").GetComponent<SpriteRenderer>();

        Sprite underSprite = Sprite.Create(mapData.UnderTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
        underSprite.texture.filterMode = FilterMode.Point;
        underRenderer.sprite = underSprite;

        SpriteRenderer upperRenderer = transform.Find("UpperSprite").GetComponent<SpriteRenderer>();

        Sprite upperSprite = Sprite.Create(mapData.UpperTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), Vector2.zero, mapData.PixelPerUnit);
        upperSprite.texture.filterMode = FilterMode.Point;
        upperRenderer.sprite = upperSprite;

        GenerateEventObjects(mapData);

        FpsFixer.FixFrameRate();
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

            EventObject eventObject = gameObject.GetComponent<EventObject>();
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
                Sprite sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f), mapData.PixelPerUnit);
                sprite.texture.filterMode = FilterMode.Point;
                spriteRenderer.sprite = sprite;
            }
        }
    }
}
