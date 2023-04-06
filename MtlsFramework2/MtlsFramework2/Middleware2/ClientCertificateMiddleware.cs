using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using EllipticCurve;
using PublicKey = EllipticCurve.PublicKey;
using System.Net.Http;
using System.Threading;
using System.Net;
using System.Configuration;
namespace MtlsFramework2.Middleware2
{
    public class ClientCertificateMiddleware : DelegatingHandler
    {

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = null;

            response = ValidateRequest(request);
            if (response != null)
            {
                return response;
            }

            response = await base.SendAsync(request, cancellationToken);

            return response;
        }


        private static HttpResponseMessage ValidateRequest(HttpRequestMessage context)
        {

            try
            {
                IEnumerable<string> headerValues = context.Headers.GetValues("MS-ASPNETCORE-CLIENTCERT");
                var certHeader = headerValues.FirstOrDefault();

                var certBytes = Convert.FromBase64String(certHeader);
                var cert = new X509Certificate2(certBytes);
                var publicKey_cert = Convert.ToBase64String(cert.GetPublicKey());
                var issuer = cert.GetNameInfo(X509NameType.DnsName, true);
                var subject = cert.GetNameInfo(X509NameType.DnsName, false);

                var _PublicKey = ConfigurationManager.AppSettings["PublicKey"].ToString();

                //PEGANDO DADOS DA REQUEST
                IEnumerable<string> accessIdValues = context.Headers.GetValues("Access-Id");
                var accessId = accessIdValues.FirstOrDefault();


                IEnumerable<string> accessTimeValues = context.Headers.GetValues("Access-Time");
                var accessTime = accessTimeValues.FirstOrDefault();

                IEnumerable<string> accessSignatureValues = context.Headers.GetValues("Access-Signature");
                var accessSignature = accessSignatureValues.FirstOrDefault();

                var message = accessId + ":" + accessTime;

                // Generate new Keys
                var privateKey = PrivateKey.fromPem("-----BEGIN EC PRIVATE KEY-----\nMHICAQEEIAcrZ0+kP2XLOL3wceKogEtOPLtuTzQQHJDyi6Ztwr4CoAcGBSuBBAAK\noUIABJoqw0BRi6vUngyJ3oYFV1bJ6ewp814wGrfyGQEmS+SDM7WHAwlqegJHin6E\npjrhpsNIMkV4bisOL0Y7TcQ9FXQ=\n-----END EC PRIVATE KEY-----");
                PublicKey publicKey = privateKey.publicKey();

                var signature = Signature.fromBase64(accessSignature);

                // Verify if signature is valid
                var result = Ecdsa.verify(message, signature, publicKey);

                if (publicKey_cert == _PublicKey)
                {
                    if (publicKey_cert != _PublicKey)
                    {
                        return context.CreateResponse(HttpStatusCode.Forbidden, "Forbiden");
                    }
                }
                else
                {
                    return context.CreateResponse(HttpStatusCode.Forbidden, "Forbiden");
                }

                return context.CreateResponse(HttpStatusCode.OK, "ok");

            }
            catch (Exception e)
            {
                return context.CreateResponse(HttpStatusCode.BadRequest, "BadRequest");
            }

        }
    }
}