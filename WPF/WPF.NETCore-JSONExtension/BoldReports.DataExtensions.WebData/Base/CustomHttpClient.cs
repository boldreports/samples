using System;
using System.Net;
using System.Net.Http;

namespace BoldReports.Web.Data.Handler
{
    public class CustomHttpClient : HttpClient
    {
        public CustomHttpClient() : base()
        {
            Timeout = TimeSpan.FromMilliseconds(240000);
            ServicePointManager.ServerCertificateValidationCallback = SupressCertificateErrors;
#if !(SyncfusionFramework4_0 || NETCore)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
#else
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif
        }

        public CustomHttpClient(HttpMessageHandler handler) : base(handler)
        {
            Timeout = TimeSpan.FromMilliseconds(240000);
            ServicePointManager.ServerCertificateValidationCallback = SupressCertificateErrors;
#if !(SyncfusionFramework4_0 || NETCore)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif
        }
        /// <summary>
        /// Supresses the certificate errors in SSL 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool SupressCertificateErrors(object sender,
            System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            System.Security.Cryptography.X509Certificates.X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

    }

	public class CustomWebClient : WebClient
	{
		public CustomWebClient() : base()
		{
			ServicePointManager.ServerCertificateValidationCallback = SupressCertificateErrors;
#if !(SyncfusionFramework4_0 || NETCore)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#endif
        }
        protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest wr = base.GetWebRequest(address);
			wr.Timeout= 240000;
			return wr;
		}
		/// <summary>
		/// Supresses the certificate errors in SSL 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="sslPolicyErrors"></param>
		/// <returns></returns>
		private bool SupressCertificateErrors(object sender,
			System.Security.Cryptography.X509Certificates.X509Certificate certificate,
			System.Security.Cryptography.X509Certificates.X509Chain chain,
			System.Net.Security.SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}
	}
}
