using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainingChatWebApp
{
    public class LoginUser
    {
        public int Key { get; set; }

        //[StringLength(25, ErrorMessage = "aesdfasdas") ]
        public string Username { get; set; }

        //[StringLength(200, ErrorMessage = "asdasdsa") ]
        public string Name { get; set; }
    }
}