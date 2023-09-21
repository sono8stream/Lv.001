using System;
using Domain.Data;
using Expression.Common;
using UnityEngine;// 【暫定】本来はログ出力のアダプタを仕込む

namespace Expression.Map.MapEvent
{
    // 【暫定】Intに限定せず、LeftHandAccessorの型に応じて計算内容を変える対応を仕込む。
    public class VariableUpdater
    {
        public IDataAccessorFactory LeftHandAccessorFactory { get; private set; }
        public IDataAccessorFactory RightHandAccessor1Factory { get; private set; }
        public IDataAccessorFactory RightHandAccessor2Factory { get; private set; }

        public OperatorType AssignOperatorType { get; private set; }
        public OperatorType RightOperatorType { get; private set; }

        public VariableUpdater(IDataAccessorFactory leftHandAccessorFactory,
            IDataAccessorFactory rightHandAccessor1Factory,
            IDataAccessorFactory rightHandAccessor2Factory,
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
            if (LeftHandAccessorFactory.TestType(context, VariableType.Number))
            {
                UpdateInt(context);
            }
            else if (LeftHandAccessorFactory.TestType(context, VariableType.String))
            {
                UpdateString(context);
            }

        }

        private void UpdateInt(CommandVisitContext context)
        {
            int rightValue1 = RightHandAccessor1Factory.GetInt(context);
            int rightValue2 = RightHandAccessor2Factory == null
                ? 0 : RightHandAccessor2Factory.GetInt(context);

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
            int leftValue = LeftHandAccessorFactory.GetInt(context);
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

            LeftHandAccessorFactory.SetInt(context, assignValue);
        }

        private void UpdateString(CommandVisitContext context)
        {
            string rightValue1 = RightHandAccessor1Factory.GetString(context);
            string rightValue2 = RightHandAccessor2Factory == null
                ? "" : RightHandAccessor2Factory.GetString(context);

            string assignValue = "";

            switch (RightOperatorType)
            {
                case OperatorType.Plus:
                    assignValue = rightValue1 + rightValue2;
                    break;
                default:
                    break;// +記号以外は何もしない
            }

            string leftValue = LeftHandAccessorFactory.GetString(context);
            switch (AssignOperatorType)
            {
                case OperatorType.NormalAssign:
                    break;
                case OperatorType.PlusAssign:
                    assignValue = leftValue + assignValue;// 追加ではなく再割り当てなのでちょっと計算コストがかかる
                    break;
                default:
                    break;
            }

            Debug.Log($"Update {leftValue} {AssignOperatorType} {rightValue1} {RightOperatorType} {rightValue2}");

            LeftHandAccessorFactory.SetString(context, assignValue);
        }
    }
}
