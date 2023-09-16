using Domain.Data;
using Expression.Common;
using UnityEngine;// 【暫定】本来はログ出力を抽象化してUnityのDebug.Logにつなぐべき

namespace Expression.Map.MapEvent
{
    public class ConditionInt
    {
        public IDataAccessorFactory<int> LeftHandAccessorFactory { get; private set; }
        public IDataAccessorFactory<int> RightHandAccessorFactory { get; private set; }

        public OperatorType OperatorType { get; private set; }

        public ConditionInt(IDataAccessorFactory<int> leftHandAccessorFactory,
            IDataAccessorFactory<int> rightHandAccessorFactory,
             OperatorType operatorType)
        {
            LeftHandAccessorFactory = leftHandAccessorFactory;
            RightHandAccessorFactory = rightHandAccessorFactory;
            OperatorType = operatorType;
        }

        public bool CheckIsTrue(Expression.Map.MapEvent.CommandVisitContext context)
        {
            int leftValue = LeftHandAccessorFactory.Get(context);
            int rightValue = RightHandAccessorFactory.Get(context);
            Debug.Log($"Compare {leftValue} & {rightValue} with {OperatorType}");

            switch (OperatorType)
            {
                case OperatorType.GreaterThan:
                    return leftValue > rightValue;
                case OperatorType.GreaterEqual:
                    return leftValue >= rightValue;
                case OperatorType.Equal:
                    return leftValue == rightValue;
                case OperatorType.LessEqual:
                    return leftValue < rightValue;
                case OperatorType.LessThan:
                    return leftValue < rightValue;
                case OperatorType.NotEqual:
                    return leftValue != rightValue;
                case OperatorType.And:
                    return (leftValue & rightValue) == rightValue;
                default:
                    break;
            }

            return false;
        }
    }

    public enum OperatorType
    {
        // 条件演算
        GreaterThan = 0,
        GreaterEqual,
        Equal,
        LessEqual,
        LessThan,
        NotEqual,
        And,// ~のビットを満たす条件に対応

        // 通常演算
        Plus,
        Minus,
        Multiply,
        Divide,
        Mod,
        Random,
        ArcTan,// 傾きを得る

        // 代入演算
        NormalAssign,
        PlusAssign,
        MinusAssign,
        MultiplyAssign,
        DivideAssign,
        ModAssign,
        MaxAssign,
        MinAssign,
        AbsAssign,
        AngleAssign,// 角度
        SinAssign,// Sin値の割り当て
        CosAssign,// Cos値の割り当て
    }
}