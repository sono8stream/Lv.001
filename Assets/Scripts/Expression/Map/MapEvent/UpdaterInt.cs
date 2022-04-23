using Domain.Data;
using Expression.Common;

namespace Expression.Map.MapEvent
{
    public class UpdaterInt
    {
        public IDataAccessor<int> LeftHandAccessor { get; private set; }
        public IDataAccessor<int> RightHandAccessor1 { get; private set; }
        public IDataAccessor<int> RightHandAccessor2 { get; private set; }
        public OperatorType AssignOperatorType { get; private set; }
        public OperatorType RightOperatorType { get; private set; }

        public UpdaterInt(IDataAccessor<int> leftHandAccessor,
            IDataAccessor<int> rightHandAccessor1,
            IDataAccessor<int> rightHandAccessor2,
            OperatorType assignOperatorType,
             OperatorType rightOperatorType)
        {
            LeftHandAccessor = leftHandAccessor;
            RightHandAccessor1 = rightHandAccessor1;
            RightHandAccessor2 = rightHandAccessor2;
            AssignOperatorType = assignOperatorType;
            RightOperatorType = rightOperatorType;
        }

        public void Update()
        {
            int rightValue1 = RightHandAccessor1.Get();
            int rightValue2 = RightHandAccessor2.Get();

            int assignValue = 0;

            switch (RightOperatorType)
            {
                case OperatorType.Plus:
                    assignValue = rightValue1 + rightValue2;
                    break;
                case OperatorType.Minus:
                    assignValue = rightValue1 - rightValue2;
                    break;
                case OperatorType.Multiply:
                    assignValue = rightValue1 * rightValue2;
                    break;
                case OperatorType.Divide:
                    assignValue = rightValue1 / rightValue2;
                    break;
                default:
                    break;
            }

            // 【暫定】全ての代入演算子に対応させる
            int leftValue = LeftHandAccessor.Get();
            switch (AssignOperatorType)
            {
                case OperatorType.NormalAssign:
                    break;
                case OperatorType.PlusAssign:
                    assignValue += leftValue;
                    break;
                case OperatorType.MinusAssign:
                    assignValue = leftValue - assignValue;
                    break;
                case OperatorType.MultiplyAssign:
                    assignValue = leftValue * assignValue;
                    break;
                case OperatorType.DivideAssign:
                    assignValue = leftValue / assignValue;
                    break;
                default:
                    break;
            }

            LeftHandAccessor.Set(assignValue);
        }
    }
}