using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserModel
    {
        /// <summary>
        /// User Id
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// User Username
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// User Email
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// User Password
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// User CreatedAt
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// User UpdatedAt
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relationship with Task entity
        public Guid TaskId { get; set; }
        public ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();
    }
}
