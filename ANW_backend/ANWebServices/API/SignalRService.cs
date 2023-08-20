using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.SignalR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANWebServices.API
{
    public class SignalRService: Hub
    {
        public readonly IRepository<Room> _room;
        public readonly IRepository<User> _user;
        public readonly IRepository<UserConnection> _con;
        public SignalRService(IRepository<Room> room, IRepository<User> user,
            IRepository<UserConnection> userConnection)
        {
            _room = room;
            _user = user;
            _con = userConnection;
        }

        /// <summary>
        /// Send warning to all other connected clients
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> sendWarning(string message)
        {
            Clients.Others.SendAsync("onWarning", message);
            return new OkResult();
        }
        
        /// <summary>
        /// Request to join a room
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ResponseMessage> joinRoom(string roomId)
        {
            string userId = Context.User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId)) return new ResponseMessage(ResponseStatus.UNAUTHORIZED);
            User participant = await _user.GetById((userId));
            Room r = await _room.GetById((roomId));
            if (r == null)
                return new ResponseMessage(ResponseStatus.BAD_REQUEST);
            HashSet<User> joinedUsers = new HashSet<User>();
            foreach (var id in r.JoinedUsers)
            {
                User u = await _user.GetById((id));
                if (u!=null)
                    joinedUsers.Add(new User()
                    {
                        Id = id,
                        DisplayName = u.DisplayName,
                        Username = u.Username,
                        AvatarImg = u.AvatarImg
                    });
            }
            var list = r.ChatLog.GroupBy(x => x.UserId).ToList();
            foreach (var item in list)
            {
                User u = await _user.GetById((item.Key));
                if (u != null)
                    joinedUsers.Add(new User()
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        Username = u.Username,
                        AvatarImg = u.AvatarImg
                    });
            }
            Clients.Clients(GetConnectionInRoom(r, Context.ConnectionId)).SendAsync("onJoined", JsonConvert.SerializeObject(new { userId, roomId, newUser= new User()
            {
                Id = userId,
                DisplayName = participant.DisplayName,
                Username = participant.Username,
                AvatarImg = participant.AvatarImg
            }
            }));
            r.JoinedUsers.Add(userId);
            _room.Update((r.Id), r);
            //Clients.Caller.SendAsync("roomInfo", JsonConvert.SerializeObject(new { room = r, chatlog = r.ChatLog }));
            return new ResponseMessage(JsonConvert.SerializeObject(new { room = r, chatlog = r.ChatLog.TakeLast(20), chatlogLength = r.ChatLog.Count, joinedUsers }));
        }

        /// <summary>
        /// Announce before leaving the room
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ResponseMessage> leaveRoom(string roomId)
        {
            string userId = Context.User.FindFirst("id")?.Value;
            Room r = await _room.GetById((roomId));
            if (r == null)
                return new ResponseMessage(ResponseStatus.BAD_REQUEST);
            r.JoinedUsers.Remove(userId);
            Clients.Clients(GetConnectionInRoom(r, Context.ConnectionId)).SendAsync("onLeft", JsonConvert.SerializeObject(new { userId, roomId }));
            _room.Update((r.Id), r);
            return new ResponseMessage(ResponseStatus.OK);
        }

        /// <summary>
        /// Send message to a room
        /// </summary>
        /// <param name="message"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<ResponseMessage> sendMessage(string message, string roomId)
        {
            string userId = Context.User.FindFirst("id")?.Value;
            if (userId == null) return new ResponseMessage(ResponseStatus.UNAUTHORIZED);
            User u = await _user.GetById((userId));
            if (u == null) return new ResponseMessage(ResponseStatus.UNAUTHORIZED);
            Room r = await _room.GetById((roomId));
            if (r == null) return new ResponseMessage(ResponseStatus.BAD_REQUEST);
            if (!r.JoinedUsers.Contains(userId)) return new ResponseMessage(ResponseStatus.BAD_REQUEST);
            var json = JsonConvert.SerializeObject(new { message, sender=u.Id });
            Clients.Clients(GetConnectionInRoom(r, Context.ConnectionId)).SendAsync("onReceivedMessage", json);
            ChatLog log = r.ChatLog.LastOrDefault();
            if (log !=null && log.UserId.Equals(userId) && log.Content.Count<15)
            {
                var current = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
                var last = new DateTimeOffset(log.UpdatedOn).ToUnixTimeSeconds();
                if (current - last < 320)
                {
                    if (current - last <= 20 && log.Content.Count>=1)
                    {
                        var target = log.Content.LastOrDefault();
                        target.Content += "<br />" + message;
                    }
                    else
                    {
                        log.Content.Enqueue(new ChatContent()
                        {
                            Content = message
                        });
                        log.UpdatedOn = DateTime.Now;
                    }
                }
                else
                {
                    if (r.ChatLog == null) r.ChatLog = new Queue<ChatLog>();
                    var chat = new ChatLog()
                    {
                        UserId = userId
                    };
                    chat.Content.Enqueue(new ChatContent()
                    {
                        Content = message
                    });
                    r.ChatLog.Enqueue(chat);
                    if (r.ChatLog.Count > 500)
                        r.ChatLog.Dequeue();
                }
            }
            else
            {
                if (r.ChatLog == null) r.ChatLog = new Queue<ChatLog>();
                var chat = new ChatLog()
                {
                    UserId = userId
                };
                chat.Content.Enqueue(new ChatContent()
                {
                    Content = message
                });
                r.ChatLog.Enqueue(chat);
                if (r.ChatLog.Count > 500)
                    r.ChatLog.Dequeue();
            }
            _room.Update((r.Id), r);
            return new ResponseMessage(ResponseStatus.OK);
        }

        public override async Task OnConnectedAsync()
        {

            try
            {
                string userId = Context.User.FindFirst("id")?.Value;
                if (userId != null && (_user.GetById((userId)).Result)!=null)
                {
                    Groups.AddToGroupAsync(Context.ConnectionId, "LoggedUsers");
                    UserConnection con =  await _con.GetById((userId));
                    if (con == null)
                    {
                        con = new UserConnection()
                        {
                            Id = userId
                        };
                        await _con.Insert(con);
                    }
                    con.Connections.Add(Context.ConnectionId);
                    _con.Update((con.Id), con);

                    var r = (await _room.Find(x => x.JoinedUsers.Contains(userId))).FirstOrDefault();
                    if (r != null)
                    {
                        var target = Clients.Client(Context.ConnectionId);
                        Task.Run(() =>
                        {
                            HashSet<User> joinedUsers = new HashSet<User>();
                            foreach (var id in r.JoinedUsers)
                            {
                                User u =  _user.GetById((id)).Result;
                                if (u != null)
                                    joinedUsers.Add(new User()
                                    {
                                        Id = id,
                                        DisplayName = u.DisplayName,
                                        Username = u.Username,
                                        AvatarImg = u.AvatarImg
                                    });
                            }
                            var list = r.ChatLog.GroupBy(x => x.UserId).ToList();
                            foreach (var item in list)
                            {
                                User u = _user.GetById((item.Key)).Result;
                                if (u != null)
                                    joinedUsers.Add(new User()
                                    {
                                        Id = u.Id,
                                        DisplayName = u.DisplayName,
                                        Username = u.Username,
                                        AvatarImg = u.AvatarImg
                                    });
                            }
                            target.SendAsync("roomInfo", JsonConvert.SerializeObject(new { room = r, chatlog = r.ChatLog.TakeLast(20) , chatlogLength=r.ChatLog.Count , joinedUsers }));
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                string userId = Context.User.FindFirst("id").Value;
                
                if (userId != null)
                {
                    Groups.RemoveFromGroupAsync(Context.ConnectionId, "LoggedUsers");
                    UserConnection con = await _con.GetById((userId));
                    con.Connections.Remove(Context.ConnectionId);
                    _con.Update((con.Id), con);
                    if (con.Connections.Count<=0)
                    {
                        List<Room> list = ((RoomDAO)_room).FindRoomHasUser(userId).Result;
                        if (list.Count>0)
                            foreach (Room r in list)
                            {
                                r.JoinedUsers.Remove(userId);
                                _room.Update((r.Id), r);
                            }
                    }
                }
            }
            catch (Exception)
            {

            }

            base.OnDisconnectedAsync(exception);
        }

        private IReadOnlyList<string> GetConnectionInRoom(Room r, string except=null)
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var id in r.JoinedUsers)
            {
                var con = _con.GetById(id).Result;
                if (con != null)
                {
                    foreach (var cid in con.Connections)
                        result.Add(cid);
                }
            }
            if (!string.IsNullOrEmpty(except))
                result.Remove(except);
            return result.ToList().AsReadOnly();
        }
    }

}
