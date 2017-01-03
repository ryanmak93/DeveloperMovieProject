using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MovieApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "Movie",
            //    url: "Movie/{id}/{action}",
            //    defaults: new { controller = "Movie"}
            //    );
            //routes.MapRoute(
            //    name: "Genre",
            //    url: "Genre/{id}/{action}",
            //    defaults: new { controller = "Genre"}
            //    );
            //routes.MapRoute(
            //    name: "User",
            //    url: "Account/{id}/{action}",
            //    defaults: new { controller = "Account"}
            //    );

            routes.MapRoute(
                name: "Route",
                url: "{controller}/{id}/{action}"
                );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Movie", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
