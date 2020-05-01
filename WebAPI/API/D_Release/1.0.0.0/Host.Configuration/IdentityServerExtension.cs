using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Host.Configuration;
using IdentityServer3.Core.Configuration;
using Microsoft.Owin.Security;

namespace Owin
{
    public static class IdentityServerExtension
    {
        public static IAppBuilder UseIdentityServer(this IAppBuilder app, string connectionString, IdentityServerOptions options)
        {
            // uncomment to enable HSTS headers for the host
            // see: https://developer.mozilla.org/en-US/docs/Web/Security/HTTP_strict_transport_security
            //app.UseHsts();

            app.Map("/core", coreApp =>
            {
                var idSvrFactory = Factory.Configure(connectionString);

                options.Factory = idSvrFactory;
                options.SigningCertificate = LoadCertificate();

                coreApp.UseIdentityServer(options);
            });

            return app;
        }

        static X509Certificate2 LoadCertificate()
        {
            var assembly = typeof(IdentityServerExtension).Assembly;
            using (var stream = assembly.GetManifestResourceStream("Host.Configuration.Config.NextGenCA.pfx"))
            {
                return new X509Certificate2(ReadStream(stream), "mercury12");
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
