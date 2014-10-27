using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Web;

namespace KeyFob
{
    public class BasicAuthenticationModule : IHttpModule
    {
        public const string HttpAuthorizationHeader = "Authorization";
        public const string HttpBasicSchemeName = "Basic"; // 
        public const char HttpCredentialSeparator = ':';
        public const int HttpNotAuthorizedStatusCode = 401;
        public const string HttpWwwAuthenticateHeader = "WWW-Authenticate";
        public const string AuthenticationCookieName = "BasicAuthentication";
        public const string Realm = "demo";

        private IDictionary<string, string> _activeUsers;

        public void AuthenticateUser(Object source, EventArgs e)
        {
            var context = ((HttpApplication) source).Context;

            var authorizationHeader = context.Request.Headers[HttpAuthorizationHeader];

            if (!ValidInputValidation(authorizationHeader))
                return;

            var authCookie = context.Request.Cookies.Get(AuthenticationCookieName);
            
            if (authCookie != null) return;
            
            authCookie = new HttpCookie(AuthenticationCookieName, "1") {Expires = DateTime.Now.AddHours(1)};
            
            context.Response.Cookies.Add(authCookie);
        }

        public void IssueAuthenticationChallenge(Object source, EventArgs e)
        {
            var context = ((HttpApplication)source).Context;

            var authorizationHeader = context.Request.Headers[HttpAuthorizationHeader];

            var authCookie = context.Request.Cookies.Get(AuthenticationCookieName);

            if (authCookie != null) return;

            if (context.Error != null)
            {
                if (ValidInputValidation(authorizationHeader))
                    return;
            }

            context.Response.Clear();
            context.Response.StatusCode = HttpNotAuthorizedStatusCode;
            context.Response.AddHeader(HttpWwwAuthenticateHeader, "Basic realm =\"" + Realm + "\"");
        }

        public bool ValidInputValidation(string authorizationHeader)
        {
            string userName = null;
            string password = null;

            return ExtractBasicCredentials(authorizationHeader, ref userName, ref password) && ValidateCredentials(userName, password);
        }

        public bool ValidateCredentials(string userName, string password)
        {
            return _activeUsers.ContainsKey(userName) && _activeUsers[userName] == password;
        }

        protected bool ExtractBasicCredentials(string authorizationHeader, ref string username, ref string password)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
                return false;

            var verifiedAuthorizationHeader = authorizationHeader.Trim();

            if (verifiedAuthorizationHeader.IndexOf(HttpBasicSchemeName, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            verifiedAuthorizationHeader =
                verifiedAuthorizationHeader.Substring(HttpBasicSchemeName.Length,
                    verifiedAuthorizationHeader.Length - HttpBasicSchemeName.Length).Trim();

            var credentialBase64DecodedArray = Convert.FromBase64String(verifiedAuthorizationHeader);
            var decodedAuthorizationHeader = Encoding.UTF8.GetString(credentialBase64DecodedArray, 0, credentialBase64DecodedArray.Length);

            var separatorPosition = decodedAuthorizationHeader.IndexOf(HttpCredentialSeparator);

            if (separatorPosition <= 0)
                return false;

            username = decodedAuthorizationHeader.Substring(0, separatorPosition).Trim();
            password = decodedAuthorizationHeader.Substring(separatorPosition + 1, (decodedAuthorizationHeader.Length - separatorPosition - 1)).Trim();

            return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        }

        public void Init(HttpApplication context)
        {
            var config = ConfigurationManager.GetSection("basicAuth");
            var basicAuth = (BasicAuthenticationConfigurationSection) config;
            _activeUsers = new Dictionary<string, string>();

            for (var i = 0; i < basicAuth.Credentials.Count; i++)
            {
                var credential = basicAuth.Credentials[i];
                _activeUsers.Add(credential.UserName, credential.Password);
            }

            context.AuthenticateRequest += AuthenticateUser;

            context.EndRequest += IssueAuthenticationChallenge;
        }

        public void Dispose()
        {
        }
    }
}