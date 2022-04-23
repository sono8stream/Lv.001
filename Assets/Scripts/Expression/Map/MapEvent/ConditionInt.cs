using Domain.Data;
using Expression.Common;

namespace Expression.Map.MapEvent
{
    public class ConditionInt
    {
        public IDataAccessor<int> LeftHandAccessor { get; private set; }
        public IDataAccessor<int> RightHandAccessor { get; private set; }

        public OperatorType OperatorType { get; private set; }

        public ConditionInt(IDataAccessor<int> leftHandAccessor,
            IDataAccessor<int> rightHandAccessor,
             OperatorType operatorType)
        {
            LeftHandAccessor = leftHandAccessor;
            RightHandAccessor = rightHandAccessor;
            OperatorType = operatorType;
        }

        public bool CheckIsTrue()
        {
            int leftValue = LeftHandAccessor.Get();
            int rightValue = RightHandAccessor.Get();

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
        Not,

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