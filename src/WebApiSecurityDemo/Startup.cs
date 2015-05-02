using System;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using System.Security.Claims;
using System.Text;
using http.basic;
using Microsoft.AspNet.Security;

namespace WebApiSecurityDemo
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }



        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            //app.Use(async (ctx, next) =>
            //{
            //    Func<string, string, bool> validation;
            //    validation = (user, pwd) => {
            //        return user == "max" && pwd == "geheim";
            //    };

            //    var tool = new HttpBasicTool();
            //    var identity = tool.Authenticate(ctx, validation);
            //    if (identity != null)
            //    {
            //        ctx.User = new ClaimsPrincipal(identity);
            //    }

            //    await next();

            //    tool.Challange(ctx);

            //});

            app.UseHttpBasic(options =>
            {
                options.ValidateCredentials = (user, pwd) =>
                {
                    return user == "max" && pwd == "geheim";
                };
            });

            app.UseOAuthBearerAuthentication(options =>
            {
                options.TokenValidationParameters.ValidAudience = "482348825399.apps.googleusercontent.com";
                options.TokenValidationParameters.ValidIssuer = "accounts.google.com";
                options.Authority = "https://accounts.google.com";
                //options.AuthenticationMode = AuthenticationMode.Active;
                
            });
                       
            app.UseStaticFiles();
            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
