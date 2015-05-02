using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.Framework.OptionsModel;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace http.basic
{
    public class HttpBasicAuthenticationOptions : AuthenticationOptions
    {
        public HttpBasicAuthenticationOptions() {
            this.AuthenticationMode = AuthenticationMode.Active;
            this.AuthenticationType = "Basic";
        }

        public Func<string, string, bool> ValidateCredentials { get; set; }
    }

    public class HttpBasicAuthenticationHandler : AuthenticationHandler<HttpBasicAuthenticationOptions>
    {
        protected override void ApplyResponseChallenge()
        {
            if (this.Response.StatusCode == 401)
            {
                Response.Headers.Add("WWW-Authenticate", new[] { "Basic" });
            }
        }

        protected override void ApplyResponseGrant()
        {
        }

        private static string DecodeBase64(string header)
        {
            header = Encoding.UTF8.GetString(Convert.FromBase64String(header));
            return header;
        }


        protected override AuthenticationTicket AuthenticateCore()
        {
            var emptyTicket = new AuthenticationTicket(null, new AuthenticationProperties());


            var header = this.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(header) ||
                    !header.ToLower().StartsWith("basic"))
            {
                return emptyTicket;
            }

            header = header.Substring(5); // Basic wegschneiden ...
            header = DecodeBase64(header);

            var index = header.IndexOf(':');
            if (index == -1)
            {
                return emptyTicket;
            }

            var user = header.Substring(0, index);
            var password = header.Substring(index + 1);

            if (Options.ValidateCredentials != null)
            {
                if (!Options.ValidateCredentials(user, password))
                {
                    return emptyTicket;
                }
            }

            var identity = new ClaimsIdentity(Options.AuthenticationType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user));

            // Weitere Claims ermitteln und setzen ...
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            var ticket = new AuthenticationTicket(identity, new AuthenticationProperties());

            return ticket;

        }
    }

    public class HttpBasicAuthenticationMiddleware : AuthenticationMiddleware<HttpBasicAuthenticationOptions>
    {
        public HttpBasicAuthenticationMiddleware(RequestDelegate next, IServiceProvider services, IOptions<HttpBasicAuthenticationOptions> options, ConfigureOptions<HttpBasicAuthenticationOptions> configureOptions) : base(next, services,options,configureOptions)
        {
        }

        protected override AuthenticationHandler<HttpBasicAuthenticationOptions> CreateHandler()
        {
            return new HttpBasicAuthenticationHandler();
        }
    }

    public static class HttpBasicApplicationBuilderExt
    {
        public static void UseHttpBasic(this IApplicationBuilder app, Action<HttpBasicAuthenticationOptions> configureOptions = null, string optionsName = "")
        {
            var options = new ConfigureOptions<HttpBasicAuthenticationOptions>(configureOptions ?? (o => { }));
            options.Name = optionsName;
            app.UseMiddleware<HttpBasicAuthenticationMiddleware>(options);
        }
    }


}