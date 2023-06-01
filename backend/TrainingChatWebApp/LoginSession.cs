using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainingChatWebApp
{
    public class LoginSession
    {
        public int Key { get; set; }
        public Guid SessionId { get; set; }
        public int UserKey { get; set; }
    }
}