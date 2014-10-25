using System;
using System.Configuration;

namespace KeyFob
{
    public class CredentialElement : ConfigurationElement
    {
        private const string UserNameAttribute = "username";
        private const string PasswordAttribute = "password";

        [ConfigurationProperty(UserNameAttribute, IsRequired = true)]
        public string UserName
        {
            get { return Convert.ToString(this[UserNameAttribute]); }
            set { this[UserNameAttribute] = value; }
        }

        [ConfigurationProperty(PasswordAttribute, IsRequired = true)]
        public string Password
        {
            get { return Convert.ToString(this[PasswordAttribute]); }
            set { this[PasswordAttribute] = value; }
        }
    }
}