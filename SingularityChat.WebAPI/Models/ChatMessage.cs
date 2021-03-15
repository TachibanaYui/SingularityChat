using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularityChat.WebAPI.Models
{
    public class ChatMessage : Message
    {
        [MaxLength(1000)]
        public string ModifiedMessage { get; set; }

        [ForeignKey("Sender")]
        public string? SenderId { get; set; }
        public User Sender { get; set; }
    }
}