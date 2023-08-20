using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/artist")]
    public class ArtistController : Controller
    {
        private readonly IRepository<Artist> _art;
        private readonly IRepository<User> _user;
        private readonly IRepository<Song> _song;
        private readonly IRepository<Permission> _p;
        private readonly IRepository<Nation> _nation;
        private readonly IHostingEnvironment _env;

        public ArtistController(IRepository<Artist> art,
            IRepository<User> user,
            IRepository<Permission> perm,
            IRepository<Song> song,
            IRepository<Nation> nation,
            IHostingEnvironment env
            )
        {
            _art = art;
            _user = user;
            _p = perm;
            _song = song;
            _nation = nation;
            _env = env;
        }

        // GET: api/<controller>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location =ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get([FromQuery] int page, [FromQuery] int size, [FromQuery] string q="")
        {
            long count = 0;
            string alphabet = null;
            if (q == null || q.Length == 0)
            {
                count = await _art.CountAll();
            }
            else
            {
                alphabet = q.ToUpper();
                count = await _art.CountIf(x => x.IsDeleted!=true && x.Deactivated!=true && x.Alphabet!=null && x.Alphabet.StartsWith(alphabet));
            }
            int maxPage = (int)Math.Ceiling(count / (double)size);
            if (page > maxPage || page <= 0)
            {
                if (q != null || q.Length > 0)
                    return NoContent();
                return NotFound();
            }
            if (maxPage == 0 || count == 0) return NoContent();
            int offset = (page - 1) * size;
            List<Artist> list = alphabet == null ?
                        await _art.GetAll(size, offset) :
                        await _art.Find(x => x.IsDeleted != true && x.Deactivated != true && x.Alphabet != null && x.Alphabet.StartsWith(alphabet), size, offset);
            foreach (var item in list)
            {
                if (item.Alphabet == null || item.Alphabet.Length==0)
                {
                    item.Alphabet = CharUtil.ConvertToNonUnicode(item.Name.Substring(0, 1)).ToUpper();
                    _art.Update((item.Id), item);
                }
                if (item.Country!=null && item.Country.Length>0)
                {
                    Nation nation = await _nation.GetById((item.Country));
                    if (nation != null)
                        item.CountryName = nation.Name;
                }
            }
            var json = JsonConvert.SerializeObject(
                new
                {
                    list,
                    maxPage,
                    page
                }
            );
            return Ok(json);
        }


        [HttpGet("manage")]
        [Authorize(Roles ="7,8,9,10")]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetNoCache([FromQuery] int page, [FromQuery] int size, [FromQuery] string q = "")
        {
            long count = 0;
            string alphabet = null;
            if (q == null || q.Length == 0)
            {
                count = await _art.CountAll();
            }
            else
            {
                alphabet = q.Trim().ToUpper();
                count = await _art.CountIf(x => x.IsDeleted != true && x.Name.ToUpper().Contains(alphabet));
            }
            int maxPage = (int)Math.Ceiling(count / (double)size);
            if (page > maxPage || page <= 0)
            {
                if (q != null || q.Length > 0)
                    return NoContent();
                return NotFound();
            }
            if (maxPage == 0 || count == 0) return NoContent();
            int offset = (page - 1) * size;
            List<Artist> list = alphabet == null ?
                        await _art.GetAll(size, offset) :
                        await _art.Find(x => x.IsDeleted != true && x.Name.ToUpper().Contains(alphabet), size, offset);
            foreach (var item in list)
            {
                if (item.Alphabet == null || item.Alphabet.Length == 0)
                {
                    item.Alphabet = CharUtil.ConvertToNonUnicode(item.Name.Substring(0, 1)).ToUpper();
                    _art.Update((item.Id), item);
                }
                if (item.Country != null && item.Country.Length > 0)
                {
                    Nation nation = await _nation.GetById((item.Country));
                    if (nation != null)
                        item.CountryName = nation.Name;
                }
            }
            var json = JsonConvert.SerializeObject(
                new
                {
                    list,
                    maxPage,
                    page
                }
            );
            return Ok(json);
        }



        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Find([FromQuery] int page, [FromQuery] int size, [FromQuery] string q)
        {
            long count = 0;
            string keyword = null;
            string[] keys = null;
            if (q == null || q.Length == 0)
            {
                count = await _art.CountAll();
            }
            else
            {
                keyword = q.ToLower().Trim();
              
                if (keyword.Contains(" ")) {
                    keys = keyword.Split(" ");
                    count = await ((ArtistDAO)_art).CountSmartSearch(keys);
                }
                else
                    count = await _art.CountIf(x => x.IsDeleted != true && x.Name.ToLower().Contains(keyword));
            }
            int maxPage = (int)Math.Ceiling(count / (double)size);
            if (page > maxPage || page <= 0) return NotFound();
            if (maxPage == 0 || count == 0) return NoContent();
            int offset = (page - 1) * size;
            List<Artist> list = keyword == null ?
                        await _art.GetAll(size, offset) :
                        keys == null ? await _art.Find(x => x.IsDeleted != true && x.Name.ToLower().Contains(keyword), size, offset) :
                        await ((ArtistDAO)_art).SmartSearch(keys, size, offset);
            foreach (var item in list)
            {
                if (item.Alphabet == null || item.Alphabet.Length == 0)
                {
                    item.Alphabet = CharUtil.ConvertToNonUnicode(item.Name.Substring(0, 1)).ToUpper();
                    _art.Update((item.Id), item);
                }
                if (item.Country != null && item.Country.Length > 0)
                {
                    Nation nation = await _nation.GetById((item.Country));
                    if (nation != null)
                        item.CountryName = nation.Name;
                }
            }
            var json = JsonConvert.SerializeObject(
                new
                {
                    list,
                    maxPage,
                    page
                }
            );
            return Ok(json);
        }

        [HttpPost("get/{id}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 90, Location = ResponseCacheLocation.Any )]
        public async Task<IActionResult> GetDetail(string id)
        {
            Artist art = await _art.GetById((id));
            if (art == null || art.IsDeleted==true) return NotFound();
            if (art.Alphabet==null || art.Alphabet.Length==0)
                art.Alphabet = CharUtil.ConvertToNonUnicode(art.Name.Substring(0, 1)).ToUpper();
            if (!string.IsNullOrEmpty(art.Country))
            {
                Nation nation = await _nation.GetById((art.Country));
                if (nation != null) art.CountryName = nation.Name;
            }
            return Ok(JsonConvert.SerializeObject(art));
        }

        [HttpPost("add")]
        [Authorize(Roles ="7,8,9,10")]
        public async Task<IActionResult> Insert(
            IFormFile file,
            string name,
            string desc,
            DateTime dob
        )
        {
            if (!ModelState.IsValid) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 6) return Forbid();

            Artist art = new Artist()
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = name,
                Desc = desc,
                DateOfBirth = dob
            };

            string fileName = file.FileName;
            var extension = fileName.Substring(fileName.Length - 3);
            var newName = CharUtil.GetRandomName(3, 4) + art.Id.ToString() + "." + extension;
            var filePath = _env.WebRootPath + "/resources/artist/" + newName;
            using (var fs = file.OpenReadStream())
            using (BufferedStream bs = new BufferedStream(fs))
            {
                long bufferSize = 124000;
                using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Read))
                {
                    byte[] fileBytes = new byte[bufferSize];
                    int readByte = 0;
                    while ((readByte = bs.ReadAsync(fileBytes, 0, (int)bufferSize).Result) != 0)
                    {
                        writer.Write(fileBytes, 0, readByte);
                        fileBytes = new byte[bufferSize];
                    }
                    writer.FlushAsync();
                }
            }

            art.AvatarImg = "artist/" + newName;
            art.Alphabet = CharUtil.ConvertToNonUnicode(art.Name.Substring(0, 1)).ToUpper();
            _art.Insert(art);

            return Ok(art);
        }

        [HttpPut("{id}")]
        [Authorize(Roles ="7,8,9,10")]
        public async Task<IActionResult> Edit(string id, [FromBody] Artist artist)
        {
            if (!ModelState.IsValid) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 6) return Forbid();
            Artist art = await _art.GetById((artist.Id));
            if (art == null) return BadRequest();
            if (p.Category < 9 && !u.Id.Equals(art.BelongTo))
                return Forbid();
            art.BelongTo = artist.BelongTo;
            art.DateOfBirth = artist.DateOfBirth;
            art.Desc = artist.Desc;
            art.Name = artist.Name;
            art.Page = artist.Page;
            art.Country = artist.Country;
            _art.Update((art.Id), art);
            if (!string.IsNullOrEmpty(art.Country)) {
                Nation nation = await _nation.GetById((artist.Country));
                if (nation != null) art.CountryName = nation.Name;
            }
            return Ok(JsonConvert.SerializeObject(art));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles ="7,8,9,10")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ModelState.IsValid) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 6) return Forbid();
            Artist art =await _art.GetById((id));
            if (art == null) return BadRequest();
            if (p.Category < 9 && !u.Id.Equals(art.BelongTo))
                return Forbid();
            Song s = (await _song.Find(x => x.Artists.Contains(art.Id))).FirstOrDefault();
            if (s == null)
            {
                if (p.Category < 9)
                    art.Deactivated = true;
                else
                    art.IsDeleted = true;
                _art.Update((art.Id), art);
            }
            else return NoContent();
            return Ok();
        }

        [HttpPost("delete")]
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> BatchDelete([FromBody] string[] Ids)
        {
            if (!ModelState.IsValid) return BadRequest();
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category <= 8) return Forbid();
            if (Ids == null) return BadRequest();
            HashSet<Artist> results = new HashSet<Artist>();
            foreach (string Id in Ids)
            {
                Artist art = await _art.GetById((Id));
                if (art != null)
                {
                    if (p.Category >= 9 || u.Id.Equals(art.BelongTo))
                    {
                        Song s = (await _song.Find(x => x.Artists.Contains(art.Id))).FirstOrDefault();
                        if (s==null)
                        {
                            if (p.Category < 9)
                                art.Deactivated = true;
                            else
                                art.IsDeleted = true;
                            _art.Update((art.Id), art);
                            results.Add(art);
                        }
                    }
                }
            }
            if (results.Count == 0) return NoContent();
            var json = JsonConvert.SerializeObject(results);
            return Ok(json);

        }

        [HttpPost("upload/avatar")]
        [Authorize(Roles ="7,8,9,10")]
        public async Task<IActionResult> UploadAvatar(string id, IFormFile file)
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 7) return Forbid();
            if (string.IsNullOrEmpty(id)) return BadRequest();
            Artist art = await _art.GetById((id));
            if (p.Category < 9 && !u.Id.Equals(art.BelongTo)) return Forbid();

            if (file == null) return BadRequest();
            var rootFolder = _env.WebRootPath + "/_media";
            if (!file.ContentType.Contains("image")) return new UnsupportedMediaTypeResult();
            if (!string.IsNullOrEmpty(art.AvatarImg) && System.IO.File.Exists(rootFolder+"/"+art.AvatarImg))
            {
                System.IO.File.Delete(rootFolder +"/" + art.AvatarImg);
            }
            var fileName = file.FileName;
            var extension = fileName.Substring(fileName.Length - 3);
            var newName = CharUtil.GetRandomName(5, 10) + art.Id.ToString() + "." + extension;
            var filePath = rootFolder + "/images/" + newName;

            if (!Directory.Exists(rootFolder))
                Directory.CreateDirectory(rootFolder);
            if (!Directory.Exists(rootFolder + "/images"))
                Directory.CreateDirectory(rootFolder + "/images");

            using (var fs = file.OpenReadStream())
            using (BufferedStream bs = new BufferedStream(fs))
            {
                long bufferSize = 1000000;
                    using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] fileBytes = new byte[bufferSize];
                        int readByte = 0;
                        while ((readByte = bs.ReadAsync(fileBytes, 0, (int)bufferSize).Result) != 0)
                        {
                            writer.Write(fileBytes, 0, readByte);
                            fileBytes = new byte[bufferSize];
                        }
                        writer.FlushAsync();
                    }
            }
            art.AvatarImg = "images/" + newName;
            _art.Update((art.Id), art);
            return Ok(art.AvatarImg);
        }


        [HttpPost("upload/background")]
        [Authorize(Roles = "7,8,9,10")]
        public async Task<IActionResult> UploadBackground(string id, IFormFile file)
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p.Category < 7) return Forbid();
            if (string.IsNullOrEmpty(id)) return BadRequest();
            Artist art = await _art.GetById((id));
            if (p.Category < 9 && !u.Id.Equals(art.BelongTo)) return Forbid();

            if (file == null) return BadRequest();
            var rootFolder = _env.WebRootPath + "/_media";
            if (!file.ContentType.Contains("image")) return new UnsupportedMediaTypeResult();
            if (!string.IsNullOrEmpty(art.BackgroundImg) && System.IO.File.Exists(rootFolder + "/" + art.BackgroundImg))
            {
                System.IO.File.Delete(rootFolder + "/" + art.BackgroundImg);
            }
            var fileName = file.FileName;
            var extension = fileName.Substring(fileName.Length - 3);
            var newName = CharUtil.GetRandomName(5, 10) + art.Id.ToString() + "." + extension;
            var filePath = rootFolder + "/images/" + newName;

            if (!Directory.Exists(rootFolder))
                Directory.CreateDirectory(rootFolder);
            if (!Directory.Exists(rootFolder + "/images"))
                Directory.CreateDirectory(rootFolder + "/images");

            using (var fs = file.OpenReadStream())
            using (BufferedStream bs = new BufferedStream(fs))
            {
                long bufferSize = 1000000;
                using (FileStream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] fileBytes = new byte[bufferSize];
                    int readByte = 0;
                    while ((readByte = bs.ReadAsync(fileBytes, 0, (int)bufferSize).Result) != 0)
                    {
                        writer.Write(fileBytes, 0, readByte);
                        fileBytes = new byte[bufferSize];
                    }
                    writer.FlushAsync();
                }
            }
            art.BackgroundImg = "images/" + newName;
            _art.Update((art.Id), art);
            return Ok(art.BackgroundImg);
        }
    }
}
