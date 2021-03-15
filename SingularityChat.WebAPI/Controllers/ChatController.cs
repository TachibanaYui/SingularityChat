using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SingularityChat.WebAPI.DTOs;
using SingularityChat.WebAPI.Models;
using SingularityChat.WebAPI.Services;

namespace SingularityChat.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        AppDbContext _dbContext;
        IMapper _mapper;
        ChatRoomServices _chatRoomServices;

        public ChatController(AppDbContext dbContext, IMapper mapper, ChatRoomServices chatRoomServices)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _chatRoomServices = chatRoomServices;
        }

        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetMessages(int roomId, int offset = 0, int limit = 50)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            var room = _dbContext.ChatRooms.Find(roomId);
            var messages = room.Messages.Where(x => x is ChatMessage || (x is SystemMessage sm && (sm.SendTo == null || sm.SendTo.Id == userId)))
                .OrderByDescending(x => x.CreatedAt)
                .Skip(offset).Take(limit)
                .Select(x => _mapper.Map<WSMessageDto>(x))
                .ToList();

            return Ok(messages);
        }


        [HttpPost("{roomId}")]
        public async Task<IActionResult> SendMessage(int roomId, [FromQuery] string message)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            var isParticipating =  _dbContext.ChatRooms.Include(x => x.ParticipatingUser)
                .Any(x => x.ParticipatingUser.Any(y => y.Id == userId));

            if(isParticipating)
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
                await _chatRoomServices.SendUserChat(roomId, user, message);

                return Ok();
            }

            return Forbid();
        }
    }
}