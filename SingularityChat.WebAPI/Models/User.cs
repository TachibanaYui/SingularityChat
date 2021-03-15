using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Models
{
    public class User : IdentityUser
    {
        public int UserTag { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public string StatusMessage { get; set; }
    }
}
