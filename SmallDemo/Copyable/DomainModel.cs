using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmallDemo.Copyable
{
    public class DM_Register
    {
        public string Mobile { get; set; }
        public string Password { get; set; }
        public int Code { get; set; }
    }

 
    public class DM_Login
    {
        public string NickName { get; set; }
        public string Password { get; set; } 
    }
}