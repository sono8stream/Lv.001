using Domain.Data;
using Expression.Common;

namespace Expression.Map.MapEvent
{
    public class UpdaterInt
    {
        public IDataAccessorFactory<int> LeftHandAccessorFactory { get; private set; }
        public IDataAccessorFactory<int> RightHandAccessor1Factory { get; private set; }
        public IDataAccessorFactory<int> RightHandAccessor2Factory { get; private set; }

        public OperatorType AssignOperatorType { get; private set; }
        public OperatorType RightOperatorType { get; private set; }

        public UpdaterInt(IDataAccessorFactory<int> leftHandAccessorFactory,
            IDataAccessorFactory<int> rightHandAccessor1Factory,
            IDataAccessorFactory<int> rightHandAccessor2Factory,
            OperatorType assignOperatorType,
             OperatorType rightOperatorType)
        {
            LeftHandAccessorFactory = leftHandAccessorFactory;
            RightHandAccessor1Factory = rightHandAccessor1Factory;
            RightHandAccessor2Factory = rightHandAccessor2Factory;
            AssignOperatorType = assignOperatorType;
            RightOperatorType = rightOperatorType;
        }

        public void Update(CommandVisitContext context)
        {
            int rightValue1 = RightHandAccessor1Factory.Create(context).Get();
            int rightValue2 = RightHandAccessor2Factory.Create(context).Get();

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
            int leftValue = LeftHandAccessorFactory.Create(context).Get();
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

            LeftHandAccessorFactory.Create(context).Set(assignValue);
        }
    }
}
