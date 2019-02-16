using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class Token
    {
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public double ExpiresIn { get; private set; }

        internal Token(string accessToken, string refreshToken, double expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresIn = expiresIn;
        }
    }
}
