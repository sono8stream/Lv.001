using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class MovableInfo
    {
        public bool IsMovable { get; private set; }

        public MovableInfo(bool isMovable)
        {
            IsMovable = isMovable;
        }
    }
}
