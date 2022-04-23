﻿using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ChoiceForkCommand : EventCommandBase
    {
        public int IndentDepth { get; private set; }
        public string[] ChoiceStrings { get; private set; }

        public ChoiceForkCommand(int indentDepth, string[] choiceStrings)
        {
            IndentDepth = indentDepth;
            ChoiceStrings = choiceStrings;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitChoiceForkCommand(this);
        }
    }
}