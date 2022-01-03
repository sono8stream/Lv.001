using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;

public class Map : MonoBehaviour
{
    [SerializeField]
    private int mapIndex;

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
}
