using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.DTOs
{
    public class WSMessageDto
    {
        public int Id { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }

        public int RoomId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
