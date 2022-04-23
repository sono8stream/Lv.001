using Domain.Data;
using Expression.Common;

namespace Expression.Map.MapEvent
{
    public class ConditionInt
    {
        public IDataAccessor<int> LeftHandAccessor { get; private set; }
        public IDataAccessor<int> RightHandAccessor { get; private set; }

        public IntOperatorType OperatorType { get; private set; }

        public ConditionInt(IDataAccessor<int> leftHandAccessor,
            IDataAccessor<int> rightHandAccessor,
             IntOperatorType operatorType)
        {
            LeftHandAccessor = leftHandAccessor;
            RightHandAccessor = rightHandAccessor;
            OperatorType = operatorType;
        }

        public bool CheckIsTrue()
        {
            int leftValue = LeftHandAccessor.Access();
            int rightValue = RightHandAccessor.Access();

            switch (OperatorType)
            {
                case IntOperatorType.GreaterThan:
                    return leftValue > rightValue;
                case IntOperatorType.GreaterEqual:
                    return leftValue >= rightValue;
                case IntOperatorType.Equal:
                    return leftValue == rightValue;
                case IntOperatorType.LessEqual:
                    return leftValue < rightValue;
                case IntOperatorType.LessThan:
                    return leftValue < rightValue;
                case IntOperatorType.NotEqual:
                    return leftValue != rightValue;
                case IntOperatorType.And:
                    return (leftValue & rightValue) == rightValue;
                default:
                    break;
            }

            return false;
        }
    }

    public enum IntOperatorType
    {
        GreaterThan = 0,
        GreaterEqual,
        Equal,
        LessEqual,
        LessThan,
        NotEqual,
        And// ~のビットを満たす条件に対応
    }
}