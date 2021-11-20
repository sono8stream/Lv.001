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
    }

    void Start()
    {
        Expression.Map.MapId mapId = new Expression.Map.MapId(mapIndex);
        mapData = Expression.WolfDependencyInjector.It().MapDataRepository.Find(mapId);

        SpriteRenderer underSprite = transform.Find("UnderSprite").GetComponent<SpriteRenderer>();

        Sprite map = Sprite.Create(mapData.UnderTexture, new Rect(0, 0, mapData.UnderTexture.width, mapData.UnderTexture.height), new Vector2(0.5f, 0.5f), mapData.PixelPerUnit);
        map.texture.filterMode = FilterMode.Point;
        underSprite.sprite = map;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
