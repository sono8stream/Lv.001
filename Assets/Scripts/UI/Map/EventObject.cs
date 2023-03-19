using UnityEngine;

namespace UI.Map
{
    /// <summary>
    /// オブジェクト情報を管理します
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

        public Expression.Map.MapEvent.EventData EventData { get; private set; }

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

        /// <summary>
        /// この処理はEventObject側で持たせる
        /// </summary>
        /// <param name="eventData"></param>
        /// <param name="spriteShader"></param>
        public void SetEventData(Expression.Map.MapEvent.EventData eventData, Shader spriteShader)
        {
            this.EventData = eventData;

            // 【暫定】イベントページ決め打ちを修正
            Texture2D currentTexture = eventData.PageData[0].GetCurrentTexture();

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

        public bool IsExecutable(Expression.Map.MapEvent.EventTriggerType triggerType)
        {
            return EventData.PageData[0].TriggerType == triggerType;
        }
    }
}
