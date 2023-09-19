using System;
using Domain.Data;
using Expression.Common;
using UnityEngine;// 【暫定】本来はログ出力のアダプタを仕込む

namespace Expression.Map.MapEvent
{
    // 【暫定】Intに限定せず、LeftHandAccessorの型に応じて計算内容を変える対応を仕込む。
    public class VariableUpdater
    {
        public IDataAccessorFactory<int> LeftHandAccessorFactory { get; private set; }
        public IDataAccessorFactory<int> RightHandAccessor1Factory { get; private set; }
        public IDataAccessorFactory<int> RightHandAccessor2Factory { get; private set; }

        public OperatorType AssignOperatorType { get; private set; }
        public OperatorType RightOperatorType { get; private set; }

        public VariableUpdater(IDataAccessorFactory<int> leftHandAccessorFactory,
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
            int rightValue1 = RightHandAccessor1Factory.Get(context);
            int rightValue2 = RightHandAccessor2Factory == null
                ? 0 : RightHandAccessor2Factory.Get(context);

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
                    assignValue = rightValue2 == 0 ? rightValue1 : rightValue1 / rightValue2;
                    break;
                default:
                    break;
            }

            // 【暫定】全ての代入演算子に対応させる
            int leftValue = LeftHandAccessorFactory.Get(context);
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
                    assignValue = assignValue == 0 ? leftValue : leftValue / assignValue;
                    break;
                case OperatorType.MaxAssign:
                    assignValue = Math.Max(leftValue, assignValue);
                    break;
                case OperatorType.MinAssign:
                    assignValue = Math.Min(leftValue, assignValue);
                    break;
                default:
                    break;
            }

            Debug.Log($"Update {leftValue} {AssignOperatorType} {rightValue1} {RightOperatorType} {rightValue2}");

            LeftHandAccessorFactory.Set(context, assignValue);

        }
    }
}
