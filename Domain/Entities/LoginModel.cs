using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class LoginModel
    {
        /// <summary>
        /// Login UserName
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Login Password
        /// </summary>
        public string Password { get; set; }
    }
}
