using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SingularityChat.WebAPI.Models;
using SingularityChat.WebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI
{
    public class ChatHub : Hub
    {
        ChatRoomServices _chatRoomService;
        AppDbContext _dbContext;
        JwtTokenService _jwtTokenService;

        public ChatHub(ChatRoomServices chatRoomServices, AppDbContext dbContext, JwtTokenService jwtTokenService)
        {
            _chatRoomService = chatRoomServices;
            _dbContext = dbContext;
            _jwtTokenService = jwtTokenService;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _chatRoomService.RemoveConnection(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            SignInWithId(userId);
            await base.OnConnectedAsync();
        }

        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("GetConnectionId", Context.ConnectionId);
        }

        private void SignInWithId(string userId)
        {
            var dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (dbUser != null)
                _chatRoomService.AddConnection(Context.ConnectionId, dbUser);
        }
    }
}
