using UnityEngine;
using System;
using System.Collections.Generic;

namespace Expression.Map.Hd2d
{
    /// <summary>
    /// 近傍マップチップを配置するうえでの制約情報リスト（4方向分）
    /// </summary>
    [System.Serializable]
    public class NeighborConstraintDict: ISerializationCallbackReceiver
    {
        public Dictionary<Direction, NeighborConstraint> constraints;

        public List<int> keys;
        public List<NeighborConstraint> values;


        public NeighborConstraintDict(Dictionary<Direction, NeighborConstraint> constraints)
        {
            if (constraints == null)
            {
                throw new System.Exception("存在しない制約情報");
            }
            if(!(constraints.ContainsKey(Direction.Up)
                && constraints.ContainsKey(Direction.Right)
                && constraints.ContainsKey(Direction.Down)
                && constraints.ContainsKey(Direction.Left)
                ))
            {
                throw new System.Exception("制約情報が不足");
            }

            this.constraints = constraints;
        }

        public NeighborConstraintDict(NeighborConstraint up,
            NeighborConstraint right, NeighborConstraint down, NeighborConstraint left)
        {
            if (up == null
                || right == null
                || left == null
                || down == null)
            {
                throw new System.Exception("制約情報が不足");
            }

            constraints = new Dictionary<Direction, NeighborConstraint>();
            constraints.Add(Direction.Up, up);
            constraints.Add(Direction.Right, right);
            constraints.Add(Direction.Down, down);
            constraints.Add(Direction.Left, left);

            keys = new List<int>();
            values = new List<NeighborConstraint>();
        }

        public NeighborConstraint this[Direction d]
        {
            get { return constraints[d]; }
            set { constraints[d] = value; }
        }

        public NeighborConstraint Get(Direction direction)
        {
            if (constraints.ContainsKey(direction))
            {
                return constraints[direction];
            }
            else
            {
                throw new System.Exception("不正な方向呼び出し");
            }
        }

        public int GetCount()
        {
            return constraints.Count;
        }

        public void OnAfterDeserialize()
        {
            constraints = new Dictionary<Direction, NeighborConstraint>();

            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++)
            {
                constraints.Add((Direction)keys[i], values[i]);
            }
        }

        public void OnBeforeSerialize()
        {
            keys = new List<int>();
            values = new List<NeighborConstraint>();

            foreach (Direction dire in constraints.Keys)
            {
                keys.Add((int)dire);
                values.Add(constraints[dire]);
            }
        }
    }
}
