using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socks5S.Plugin
{
    public struct AuthenticationInformation
    {

        public readonly bool Success;
        public readonly long Id;
        public readonly string Username;
        public readonly string Email;
        public readonly double Balance;
        public readonly SubscriptionType Subscription;
        public readonly DateTime ValidTill;

        /// <summary>
        /// Create default authentication information with Success = false
        /// </summary>
        public AuthenticationInformation(bool success)
        {
            this.Success = success;
            this.Id = 0;
            this.Username = string.Empty;
            this.Email = string.Empty;
            this.Balance = 0.0;
            this.Subscription = SubscriptionType.Anonymous;
            this.ValidTill = DateTime.Now;
        }

        /// <summary>
        /// Create authentication information with details and Success = true
        /// </summary>
        public AuthenticationInformation(long id, string username, string email, double balance, SubscriptionType subscription, DateTime validTill)
        {
            this.Success = true;
            this.Id = id;
            this.Username = username;
            this.Email = email;
            this.Balance = balance;
            this.Subscription = subscription;
            this.ValidTill = validTill;
        }
        

    }
}
