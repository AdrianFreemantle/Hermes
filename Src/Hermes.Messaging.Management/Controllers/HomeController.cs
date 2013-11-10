using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using ServiceStack.OrmLite;

namespace Hermes.Messaging.Management.Controllers
{
    public class TableSchema
    {
        public string Schema { get; set; }
        public string Table { get; set; }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            IDbConnectionFactory dbFactory = new OrmLiteConnectionFactory("Data Source=.\\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=SSPI", SqlServerDialect.Provider);

            using (IDbConnection db = dbFactory.OpenDbConnection())
            {
                var result = db.Select<TableSchema>("SELECT TABLE_SCHEMA AS 'Schema', TABLE_NAME AS 'Table' FROM INFORMATION_SCHEMA.TABLES order by TABLE_SCHEMA");

                foreach (var tableSchema in result)
                {
                    System.Diagnostics.Trace.WriteLine(tableSchema.Table);
                }
            }

            return View();
        }
    }
}
