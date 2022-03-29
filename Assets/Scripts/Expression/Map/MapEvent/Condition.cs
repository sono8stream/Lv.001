using Domain.Data;
using Expression.Common;

namespace Expression.Map.MapEvent
{
    public class Condition<T>
    {
        public IDataAccessor<T> LeftHandAccessor { get; private set; }
        public IDataAccessor<T> RightHandAccessor { get; private set; }

        public OperatorType OperatorType { get; private set; }

        public Condition(IDataAccessor<T> leftHandAccessor, IDataAccessor<T> rightHandAccessor,
             OperatorType operatorType)
        {
            LeftHandAccessor = leftHandAccessor;
            RightHandAccessor = rightHandAccessor;
            OperatorType = operatorType;
        }
    }

    public enum OperatorType
    {
        GreaterThan = 0,
        GreaterEqual,
        LessEqual,
        LessThan,
        NotEqual,
        And// ~のビットを満たす条件に対応
    }
}