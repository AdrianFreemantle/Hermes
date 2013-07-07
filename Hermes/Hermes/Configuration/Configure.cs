namespace Hermes.Configuration
{
    public class Configure
    {
        private static readonly Configure instance;

        static Configure()
        {
            instance = new Configure();
        }

        private Configure()
        {
            
        }

        public static Configure With()
        {
            return instance;
        }        
    }
}