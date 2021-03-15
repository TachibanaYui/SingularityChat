using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SingularityChat.WebAPI.DTOs;
using SingularityChat.WebAPI.Models;
using SingularityChat.WebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SingularityChat.WebAPI.Services
{
    public class ChatRoomServices : IDisposable
    {
        public int AutoGenerateLimit { get; set; } = 3;
        public TimeSpan DefaultLifeSpawn { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan UpdateInterval { get; private set; } = TimeSpan.FromSeconds(1);
        public ImmutableDictionary<string, User> ConnectionInfo => _connectionInfo.ToImmutableDictionary();

        private Task _updateTask;
        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private IHubContext<ChatHub> _gateway;
        private AppDbContext _dbContext;
        private IServiceScope _scope;
        private IMapper _mapper;
        private Dictionary<string, User> _connectionInfo = new Dictionary<string, User>();

        public ChatRoomServices(IServiceProvider sp)
        {
            _scope = sp.CreateScope();
            _mapper = _scope.ServiceProvider.GetRequiredService<IMapper>();
            _gateway = _scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub>>();
            _dbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _updateTask = Task.Run(async () =>
            {
                while(!_cancel.Token.IsCancellationRequested)
                {
                    await Update();
                    await Task.Delay(UpdateInterval, _cancel.Token);
                }
            }, _cancel.Token);
        }

        // Used by client/controller
        public async Task JoinRoom(int roomId, User user, bool requireUpdate = true)
        {
            if(requireUpdate) await Update();
            var requestedRoom = _dbContext.ChatRooms
                .Include(x => x.ParticipatingUser)
                .FirstOrDefault(x => x.Id == roomId);
            var requestedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            if(requestedRoom != null && requestedUser != null)
            {
                requestedRoom.ParticipatingUser.Add(requestedUser);
                await _dbContext.SaveChangesAsync();
                foreach (var x in GetRoomConnectionIds(requestedRoom))
                {
                    await x.SendAsync("UserJoined", roomId, user.Id);
                }
            }
        }

        // Used by client/controller
        // different context so we cannot use the Room object controller send to us
        public async Task LeaveRoom(int roomId, User user, bool requireUpdate = true)
        {
            if (requireUpdate) await Update();
            var requestedRoom = _dbContext.ChatRooms.FirstOrDefault(x => x.Id == roomId);
            var requestedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            if (requestedRoom != null && requestedUser != null)
            {
                await _gateway.Clients.Client(GetConnectionId(requestedUser)).SendAsync("Leave", roomId, user.Id);
                await _dbContext.SaveChangesAsync();
                foreach (var item in GetRoomConnectionIds(requestedRoom))
                {
                    await item.SendAsync("UserLeaved", roomId, user.Id);
                }
            }
        }

        public async Task SendUserChat(int roomId, User user, string msg, bool requireUpdate = true)
        {
            if (requireUpdate) await Update();
            var requestedRoom = _dbContext.ChatRooms
                .Include(x => x.Messages)
                .Include(x => x.ParticipatingUser)
                .FirstOrDefault(x => x.Id == roomId);
            var requestedUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            if (requestedRoom != null && requestedUser != null && requestedRoom.ParticipatingUser.Any(x => x.Id == user.Id))
            {
                var msgObj = new ChatMessage()
                {
                    Content = msg,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedMessage = msg,
                    Sender = requestedUser,
                    SenderId = requestedUser.Id,
                    Room = requestedRoom
                };

                requestedRoom.Messages.Add(msgObj);
                await _dbContext.SaveChangesAsync();
                var chatMsg = _mapper.Map<ChatMessage, WSChatMessageDto>(msgObj);
                foreach(var item in GetRoomConnectionIds(requestedRoom))
                {
                    await item.SendAsync("UserChat", chatMsg);
                }
            }
        }

        public async Task<int> CreateChatRoom(bool requireUpdate = true)
        {
            if (requireUpdate) await Update();
            ChatRoom room = new ChatRoom()
            {
                CreatedAt = DateTime.UtcNow,
                Name = Consts.GetRandomRoomNames(),
                Description =  "Cool and good",
                // We don't set end of life until at least 1 user chat on the chatroom
            };

            _dbContext.ChatRooms.Add(room);
            await _dbContext.SaveChangesAsync();
            await _gateway.Clients.All.SendAsync("ChatRoomCreated", room.Id);
            return room.Id;
        }

        public async Task DeleteChatRoom(ChatRoom room, bool requireUpdate = true)
        {
            if (requireUpdate) await Update();

            var dbRoom = _dbContext.ChatRooms.FirstOrDefault(x => x.Id == room.Id);
            if (dbRoom != null)
            {
                foreach (var user in room.ParticipatingUser)
                {
                    await _gateway.Clients.Client(GetConnectionId(user))?.SendAsync("Kick");
                }

                await _gateway.Clients.All.SendAsync("ChatRoomDeleted", room.Id);
                _dbContext.ChatRooms.Remove(dbRoom);
            }
        }

        public string GetConnectionId(User user)
        {
            foreach(var entry in _connectionInfo)
            {
                if(entry.Value == user)
                {
                    return entry.Key;
                }
            }

            return null;
        }

        public async Task Update()
        {
            var expired = _dbContext.ChatRooms.Where(x => x.EstimatedEndOfLife == DateTime.UtcNow).ToList();
            foreach (var item in expired)
            {
                await DeleteChatRoom(item, false);
            }

            while (_dbContext.ChatRooms.Count() < AutoGenerateLimit)
            {
                await CreateChatRoom(false);
            }
        }

        public void AddConnection(string connectionId, User user)
        {
            var dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == user.Id);
            if(dbUser != null)
            {
                _connectionInfo.Add(connectionId, dbUser);
            }
        }

        public void RemoveConnection(string connectionId)
        {
            _connectionInfo.Remove(connectionId);
        }

        private IEnumerable<IClientProxy> GetRoomConnectionIds(ChatRoom room) 
            => room.ParticipatingUser
                .Select(x => GetConnectionId(x))
                .Where(x => x != null)
                .Select(x => _gateway.Clients.Client(x));

        public void Dispose()
        {
            _cancel.Cancel();
            _scope.Dispose();
        }
    }
}
