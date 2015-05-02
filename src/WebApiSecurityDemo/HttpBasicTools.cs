using Microsoft.AspNet.Http;
using System;
using System.Security.Claims;
using System.Text;

namespace WebApiSecurityDemo
{
    public class HttpBasicTool
    {
        public string AuthType { get; set; } = "Basic";

        private string DecodeBase64(string header)
        {
            header = Encoding.UTF8.GetString(Convert.FromBase64String(header));
            return header;
        }

        public ClaimsIdentity Authenticate(HttpContext ctx, Func<string, string, bool> validate)
        {
            var header = ctx.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(header) ||
                    !header.ToLower().StartsWith("basic"))
            {
                return null;
            }

            header = header.Substring(5); // Basic wegschneiden ...
            header = DecodeBase64(header);

            var index = header.IndexOf(':');
            if (index == -1)
            {
                return null;
            }

            var user = header.Substring(0, index);
            var password = header.Substring(index + 1);

            if (validate != null)
            {
                if (!validate(user, password))
                {
                    return null;
                }
            }

            var identity = new ClaimsIdentity(AuthType);
            identity.AddClaim(new Claim(ClaimTypes.Name, user));

            // Weitere Claims ermitteln und setzen ...
            identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            return identity;

        }

        public void Challange(HttpContext ctx)
        {
            if (ctx.Response.StatusCode == 401)
            {
                ctx.Response.Headers.Add("WWW-Authenticate", new[] { "Basic" });
            }

        }
    }
}