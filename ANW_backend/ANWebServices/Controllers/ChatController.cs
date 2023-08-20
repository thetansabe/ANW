using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/chat")]
    [Authorize]
    public class ChatController : Controller
    {
        public readonly IRepository<Room> _room;
        public readonly IRepository<User> _user;
        public readonly IRepository<Permission> _p;
        private readonly IHubContext<SignalRService> _signalR;

        public ChatController(IRepository<Room> room, IRepository<User> user,
             IHubContext<SignalRService> signalR,
            IRepository<Permission> permission)
        {
            _room = room;
            _user = user;
            _p = permission;
            _signalR = signalR;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoom()
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            List<Room> rooms = null;
            if (p.Category < 9)
            {
                rooms = await _room.Find(x=>x.Publicity>0 || x.CreatedBy.Equals(u.Id) || x.JoinedUsers.Contains(u.Id));// rooms.FindAll(x => x.Publicity > 0 || x.CreatedBy.Equals(u.Id));
            }
            else
            {
                rooms= await _room.GetAllAsync();
            }
            var json = JsonConvert.SerializeObject(rooms);
            return Ok(json);
        }
        
        [HttpPost("room/create")]
        public async Task<IActionResult> CreateRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 9)
            {
                if (room.Publicity > 5) room.Publicity = 1;
                room.Capacity = 100;
            }
            room.CreatedBy = u.Id;
            room.JoinedUsers = new HashSet<string>();
            _room.Insert(room);
            _signalR.Clients.Group("LoggedUsers").SendAsync("onRoomCreated", JsonConvert.SerializeObject(room));
            return Ok(JsonConvert.SerializeObject(room));
        }

        [HttpPut("room/edit")]
        public async Task<IActionResult> EditRoom([FromBody] Room room)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Room r = await _room.GetById((room.Id));
            if (r == null) return BadRequest();
            Permission p = await _p.GetById((u.Permission));
            r.Capacity = room.Capacity;
            r.Name = room.Name;
            r.Publicity = room.Publicity;
            r.QueueMode = room.QueueMode;
            r.AllowChat = room.AllowChat;
            r.AllowVoice = room.AllowVoice;
            _room.Update((r.Id), r);
            if (r.Publicity>0)
                _signalR.Clients.Group("LoggedUsers").SendAsync("onRoomCreated", JsonConvert.SerializeObject(room));
            return Ok(JsonConvert.SerializeObject(r));
        }

        [HttpPut("room/delete/{id}")]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Room r = await _room.GetById((id));
            if (r == null) return BadRequest();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 9 && !r.CreatedBy.Equals(u.Id))
            {
                return Forbid();
            }

            _room.Delete((id));
            _signalR.Clients.Group("LoggedUsers").SendAsync("onRoomDeleted", JsonConvert.SerializeObject(r));
            return Ok(JsonConvert.SerializeObject(r));
        }
    }
}
