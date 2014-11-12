using System;
using EntityFrameworkTest.Model;
using Hermes.EntityFramework;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace EntityFrameworkTest
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UserNameResolver(GetCurrentUserName)
                .UseSqlTransport("SqlTransport")
                .ConfigureEntityFramework<EntityFrameworkTestContext>("EntityFrameworkTest");
        }

        private static string GetCurrentUserName()
        {
            return Environment.UserName;
        }
    }
}