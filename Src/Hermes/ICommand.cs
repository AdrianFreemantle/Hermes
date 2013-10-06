using System;

namespace Hermes
{
    public interface ICommand : IMessage
    {
        Guid CommandId { get; }
    }

    public class Command : ICommand
    {
        public Guid CommandId { get; private set; }

        protected Command()
        {
            CommandId = SequentialGuid.New();
        }
    }
}