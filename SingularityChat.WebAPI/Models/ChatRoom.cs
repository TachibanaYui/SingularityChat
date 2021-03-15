using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Models
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime EstimatedEndOfLife { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [InverseProperty("Room")]
        public List<Message> Messages { get; set; }

        public List<User> ParticipatingUser { get; set; }
    }
}
