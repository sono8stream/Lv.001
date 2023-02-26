using UnityEngine;

namespace Expression.Map.Hd2d
{
    /// <summary>
    /// 近傍マップチップを配置するうえでの制約情報を格納
    /// </summary>
    [System.Serializable]
    public class NeighborConstraint
    {
        public bool hasConstraint;
        public Vector3Int offset;

        public NeighborConstraint()
        {
            hasConstraint = false;
            offset = Vector3Int.zero;
        }

        public NeighborConstraint(bool hasConstraint, Vector3Int offset)
        {
            this.hasConstraint = hasConstraint;
            this.offset = offset;
        }
    }
}
