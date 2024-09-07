using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RegisterModel
    {
        /// <summary>
        /// Register Username 
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Register Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Register Password
        /// </summary>
        public string Password { get; set; }
    }
}
