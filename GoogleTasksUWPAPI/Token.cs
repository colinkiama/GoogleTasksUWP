using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class Token
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset ExpiryTime { get; set; }
        
    

        internal Token(string accessToken, string refreshToken, double expiresIn)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiryTime = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        }
    }
}
