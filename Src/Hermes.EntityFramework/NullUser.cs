namespace Hermes.EntityFramework
{
    internal class NullUser : ICurrentUser
    {
        public string UserName
        {
            get { return string.Empty; }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }
    }
}