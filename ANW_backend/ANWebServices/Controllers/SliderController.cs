using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/slider")]
    public class SliderController : Controller
    {
        private readonly IRepository<Slider> _slider;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        private readonly IHostingEnvironment _env;
        private readonly IHubContext<SignalRService> _signalR;
        public SliderController(IRepository<Slider> val, IRepository<User> user, 
            IRepository<Permission> p, IHostingEnvironment env,
            IHubContext<SignalRService> hub
        )
        {
            _slider = val;
            _user = user;
            _p = p;
            _env = env;
            _signalR = hub;
        }

        // GET: api/<controller>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get()
        {
            HashSet<Slider> list = (await ((SliderDAO)_slider).GetAllInRange(DateTime.Now)).ToHashSet();
            if (list.Count <= 1)
            {
                list.UnionWith(await (_slider).GetAll(2 - list.Count, 0));
            }

            var json = JsonConvert.SerializeObject(list);
            return Ok(json);
        }

        [HttpGet("{id}")]
        [Authorize(Roles ="10,9")]
        public async Task<IActionResult> Get(string id)
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8)
                return Forbid();
            Slider slide = await _slider.GetById((id));
            if (slide == null) return NotFound();
            return Ok(JsonConvert.SerializeObject(slide));
        }


        [HttpGet("manage")]
        [Authorize(Roles ="10,9")]
        public async Task<IActionResult> GetAll()
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8)
                return Forbid();
            List<Slider> list = await _slider.GetAllAsync();
            var json = JsonConvert.SerializeObject(list);
            return Ok(json);
        }

        [HttpDelete("delete/{id}")]
        [Authorize(Roles ="10,9")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                string userId = HttpContext.User.FindFirst("id").Value;
                User u = await _user.GetById((userId));
                if (u == null) return Unauthorized();
                Permission p = await _p.GetById((u.Permission));
                if (p.Category <= 8)
                    return Forbid();
                Slider sli = await _slider.GetById((id));
                if (sli == null) return BadRequest();
                bool b= await _slider.Delete((sli.Id));
                string dir = _env.WebRootPath + "/resources/image/" + sli.Path;
                if (System.IO.File.Exists(dir) && b)
                {
                    try
                    {
                        System.IO.File.Delete(dir);
                    }
                    catch (Exception) { }
                }
                if (b)
                {
                    this.UpdateToClient();
                    return Ok();
                }
                else return new StatusCodeResult(304); // not modified
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest();
            }
        }

        [HttpPut("upload")]
        [Authorize(Roles ="10,9")]
        public async Task<IActionResult> Upload(
            string title,
            string desc,
            string alignment,
            string validFrom,
            string validTo,
            IFormFile file
        )
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8)
                return Forbid();
            try
            {
                var fileName = file.FileName;
                var extension = Path.GetExtension(fileName);
                string newName = CharUtil.GetRandomName(32, 40)  + extension;
                string dir = _env.WebRootPath + "/resources/image/";
                string filePath= dir+ newName;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                using (Stream fs = file.OpenReadStream())
                    using (BufferedStream bs= new BufferedStream(fs))
                {
                    int bufferSize = 4048;
                    using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] fileBytes = new byte[bufferSize];
                        int readByte = 0;
                        while ((readByte = await bs.ReadAsync(fileBytes, 0, (int)bufferSize)) != 0)
                        {
                            writer.Write(fileBytes, 0, readByte);
                            fileBytes = new byte[bufferSize];
                        }
                        writer.FlushAsync();
                    }
                }
                DateTime from = Convert.ToDateTime(validFrom);
                DateTime to = Convert.ToDateTime(validTo);
                Slider sli = new Slider()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Title = title,
                    Desc = desc,
                    Alignment = alignment,
                    Path = newName,
                    ValidFrom = from,
                    ValidTo = to
                };
                _slider.Insert(sli);

                this.UpdateToClient();

                var json = JsonConvert.SerializeObject(sli);


                return Ok(json);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("edit")]
        [Authorize(Roles ="10,9")]
        public async Task<IActionResult> Edit([FromBody] Slider model)
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8)
                return Forbid();
            try
            {
                await _slider.Update((model.Id), model);
                this.UpdateToClient();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public async Task UpdateToClient()
        {
            List<Slider> list = await ((SliderDAO)_slider).GetAllInRange(DateTime.Now);
            if (list.Count <= 1)
            {
                list.AddRange(await (_slider).GetAll(4 - list.Count, 0));
            }
            var signalRResult = JsonConvert.SerializeObject(list);

            this._signalR.Clients.All.SendAsync("onSliderChanged", signalRResult);
        }
    }
}
