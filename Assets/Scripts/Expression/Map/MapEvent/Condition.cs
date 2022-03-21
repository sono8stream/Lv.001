using Domain.Data;

namespace Expression.Map.MapEvent
{
    public class Condition
    {
        public DataRef LeftHandId { get; private set; }
        public DataRef RightHandId { get; private set; }
        public int RightHandVal { get; private set; }

        public bool IsRightConstant { get; private set; }

        public OperatorType OperatorType { get; private set; }

        public Condition(DataRef leftHandId, DataRef rightHandId, int rightHandVal,
             bool isRightConstant, OperatorType operatorType)
        {
            LeftHandId = leftHandId;
            RightHandId = rightHandId;
            RightHandVal = rightHandVal;
            IsRightConstant = isRightConstant;
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