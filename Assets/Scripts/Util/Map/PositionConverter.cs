using UnityEngine;

namespace Util.Map
{
    public class PositionConverter
    {
        public PositionConverter()
        {

        }

        public static Vector2 GetNormalizedUnityPos(Vector2 unityPos)
        {
            return new Vector2(Mathf.Floor(unityPos.x) + 0.5f, Mathf.Floor(unityPos.y) + 0.5f);
        }

        public static Vector2 GetUnityPos(Vector2Int generalPos, int mapHeight)
        {
            float nextX = generalPos.x + 0.5f;

            float nextY = mapHeight - generalPos.y - 0.5f;

            return new Vector2(nextX, nextY);
        }

        public static Vector2Int GetGeneralPos(Vector2 unityPos, int mapHeight)
        {
            int nextX = Mathf.FloorToInt(unityPos.x);
            int nextY = mapHeight - Mathf.FloorToInt(unityPos.y) - 1;

            return new Vector2Int(nextX, nextY);
        }

        public static Vector3 GetUnityHd2dPos(Vector2Int pos, int mapHeight)
        {
            Vector3 res = new Vector3();
            res.x = pos.x;
            res.y = 1;
            res.z = mapHeight - pos.y - 1;

            return res;
        }
    }
}