using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.DTOs
{
    public class WSChatMessageDto : WSMessageDto
    {
        public string? SenderId { get; set; }
    }
}
