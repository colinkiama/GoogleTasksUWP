using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public delegate void TokenEventHandler (TokenEventArgs args);

    public sealed class TokenEventArgs
    {
        public TokenEventArgs(Token token)
        {
            GeneratedToken = token;
        }
        public Token GeneratedToken { get; }
    }
}
