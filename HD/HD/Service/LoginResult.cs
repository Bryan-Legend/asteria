using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HD
{
    public class LoginResult
    {
        public string AccountName { get; set; }
        public long AccountId { get; set; }
        public long SessionId { get; set; }

        public bool IsDemo { get; set; }
        public int DemoMinutesLeft { get; set; }

        public string ErrorMessage { get; set; }
    }
}
