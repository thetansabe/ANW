using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/nation")]
    public class NationController : Controller
    {

        private readonly IRepository<Nation> _nation;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        private readonly IRepository<Artist> _art;
        public NationController(
            IRepository<Nation> nation,
            IRepository<User> user,
            IRepository<Permission> perm,
            IRepository<Artist> artist
            )
        {
            _nation = nation;
            _user = user;
            _p = perm;
            _art = artist;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 320 )]
        public async Task<IActionResult> Get()
        {
            var list = await _nation.GetAllAsync();
            if (list == null || list.Count == 0) return NoContent();
            return Ok(JsonConvert.SerializeObject(list));
        }

        [HttpPost]
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> Update(Nation which)
        {
            if (!ModelState.IsValid || which==null) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();
            if (p.Category < 9) return Forbid();
            Nation nation = null;
            if (which.Id != null)
            {
                nation = await _nation.GetById((which.Id));
            }

            if (nation != null)
            {
                nation.Name = which.Name;
                _nation.Update((nation.Id), nation);
            }
            else
            {
                nation = new Nation()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = which.Name
                };
                _nation.Insert(nation);
            }

            return Ok(JsonConvert.SerializeObject(nation));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> Delete(string Id)
        {
            if (!ModelState.IsValid) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();
            if (p.Category < 9) return Forbid();

            Nation nation = await _nation.GetById((Id));
            if (nation == null) return NotFound();
            long count = await _art.CountIf(artist => artist.Country != null && artist.Country.Equals(nation.Id));
            if (count > 0) return new ConflictResult();
            _art.Delete((nation.Id));
            return Ok();

        }
    }
}
