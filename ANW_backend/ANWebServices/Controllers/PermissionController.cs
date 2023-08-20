using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/permission")]
    public class PermissionController : Controller
    {

        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        public PermissionController(IRepository<User> val3,
            IRepository<Permission> perm)
        {
            _user = val3;
            _p = perm;
        }

        [HttpGet]
        [Authorize(Roles = "10")]
        public async Task<IActionResult> Get()
        {

            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 10)
                return Forbid();
            var json = JsonConvert.SerializeObject(await _p.GetAllAsync());
            return Ok(json);
        }

        [HttpPost("my")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p != null)
                return Ok(JsonConvert.SerializeObject(p));
            else return BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "10")]
        public async Task<IActionResult> Create([FromBody] Permission model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 10)
                return Forbid();
            if (model.Category == null || model.Category == 10)
                return BadRequest();
            if (model.Name == null)
                return BadRequest();
            model.Id = ObjectId.GenerateNewId().ToString();
            _p.Insert(model);
            return Ok(JsonConvert.SerializeObject(model));
        }

        [HttpPut]
        [Authorize(Roles = "10,9")]
        public async Task<IActionResult> Edit([FromBody] Permission model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= model.Category)
                return Forbid();
            Permission target = await _p.GetById((model.Id));
            if (target == null) return NotFound();
            if (model.Category == null || model.Category == 10)
                return BadRequest();
            if (model.Name == null)
                return BadRequest();
            target.Name = model.Name;
            target.Desc = model.Desc;
            target.Price = model.Price;
            target.Category = model.Category;
            _p.Update((target.Id), target);
            return Ok(JsonConvert.SerializeObject(target));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="10")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 10)
                return Forbid();
            Permission target = await _p.GetById((id));
            if (target == null) return NotFound() ;
            if (target.Id == p.Id)
                return BadRequest();
            long count = await ((UserDAO)_user).CountUserWithPermission(target.Id);
            if (count > 0)
                return BadRequest();
            _p.Delete((target.Id));

            return Ok();
        }
    }
}
