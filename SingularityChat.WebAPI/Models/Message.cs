using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public ChatRoom Room { get; set; }

        [MaxLength(1000)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class SystemMessage : Message
    {
        // Null for all users
        public User SendTo { get; set; }
    }
}
