using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace UI.Map
{
    /// <summary>
    /// イベント情報を保持し、動作制御します
    /// </summary>
    public class EventObject : MonoBehaviour
    {
        [SerializeField]
        bool canThrough;

        public bool CanThrough
        {
            get { return canThrough; }
            set { canThrough = value; }
        }

        [SerializeField]
        Sprite[] sprites;

        [SerializeField]
        bool isAnimating;

        int aniLimit = 60;
        int aniCount;
        int spriteCount;

        private Expression.Map.MapEvent.EventData eventData;

        // Use this for initialization
        void Start()
        {
            aniCount = 0;
            spriteCount = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (isAnimating)
            {
                aniCount++;
                if (aniLimit < aniCount)
                {
                    aniCount = 0;
                    spriteCount++;
                    if (spriteCount >= sprites.Length * 2 - 2)
                    {
                        spriteCount = 0;
                    }
                    int spriteI = spriteCount >= sprites.Length ? sprites.Length * 2 - 2 - spriteCount : spriteCount;
                    GetComponent<SpriteRenderer>().sprite = sprites[spriteI];
                }
            }
        }

        public bool IsExecutable(Expression.Map.MapEvent.EventTriggerType triggerType)
        {
            return eventData.PageData[0].TriggerType == triggerType;
        }

        public void SetEventData(Expression.Map.MapEvent.EventData eventData, Shader spriteShader)
        {
            this.eventData = eventData;

            Texture2D currentTexture = eventData.PageData[0].GetCurrentTexture();

            if (currentTexture == null)
            {
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
            else
            {
                Material mat = new Material(spriteShader);
                mat.mainTexture = currentTexture;
                mat.mainTexture.filterMode = FilterMode.Point;
                GetComponentInChildren<Renderer>().sharedMaterial = mat;
            }
        }
    }
}