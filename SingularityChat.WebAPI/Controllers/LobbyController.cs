using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class LobbyController : ControllerBase
    {
        IMapper _mapper;
        AppDbContext _dbContext;
        ChatRoomServices _chatRoomServices;

        public LobbyController(IMapper mapper, AppDbContext dbContext, ChatRoomServices chatRoomServices)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _chatRoomServices = chatRoomServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatRoomsAsync(int offset = 0, int limit = 10)
        {
            var rooms = _dbContext.ChatRooms
                .Skip(offset).Take(limit)
                .ToList()
                .Select(x => _mapper.Map<ChatRoom, RoomReadDto>(x))
                .ToList();

            return Ok(rooms);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatRoomAsync(int id)
        {
            var room = _dbContext.ChatRooms.Where(x => x.Id == id).FirstOrDefault();
            return room == null ? (IActionResult) NotFound() : Ok(_mapper.Map<ChatRoom, RoomReadDto>(room));
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> JoinAsync(int id)
        {
            var claimId = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            if(claimId != null)
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Id == claimId.Value);
                if(user != null)
                {
                    await _chatRoomServices.JoinRoom(id, user);
                    return Ok();
                }
            }

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> LeaveAsync()
        {
            var claimId = User.Claims.FirstOrDefault(x => x.Type == "UserId");
            if (claimId != null)
            {
                var user = _dbContext.Users.FirstOrDefault(x => x.Id == claimId.Value);
                var room = _dbContext.ChatRooms
                    .Include(x => x.ParticipatingUser)
                    .FirstOrDefault(x => x.ParticipatingUser.Any(x => x.Id == user.Id));
                if (user != null && room != null)
                {
                    await _chatRoomServices.JoinRoom(room.Id, user);
                    return Ok();
                }
            }

            return Forbid();
        }
    }
}