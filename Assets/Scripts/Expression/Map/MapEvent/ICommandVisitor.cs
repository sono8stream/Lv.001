using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public interface ICommandVisitor
    {
        public void OnVisitMessageCommand(MessageCommand command);
        public void OnVisitChoiceForkCommand(ChoiceForkCommand command);
        public void OnVisitForkBeginCommand(ForkBeginCommand command);
        public void OnVisitForkEndCommand(ForkEndCommand command);
        public void OnVisitForkByVariableIntCommand(ForkByVariableIntCommand command);
        public void OnVisitChangeVariableIntCommand(ChangeVariableIntCommand command);
        public void OnVisitBaseCommand(EventCommandBase command);
        public void OnVisitMovePositionCommand(MovePositionCommand command);
        public void OnVisitShowPictureCommand(Command.ShowPictureCommand command);
        public void OnVisitRemovePictureCommand(Command.RemovePictureCommand command);
        public void OnVisitCallEventCommand(Command.CallWolfEventCommand command);

    }
}