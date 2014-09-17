namespace Hermes.Messaging
{
    public interface IValidateCommand<in TCommand> where TCommand : class
    {
        void Validate(TCommand command);
    }
}