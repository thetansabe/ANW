using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [Route("api/album")]
    public class AlbumController : Controller
    {
        private readonly IRepository<Song> _song;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        private readonly IRepository<Artist> _artist;
        private readonly IRepository<Album> _alb;
        private readonly IHostingEnvironment _env;
        public AlbumController(IRepository<Song> val, IRepository<Artist> val2,
            IRepository<User> val3, IRepository<Album> alb,
            IRepository<Permission> perm, IHostingEnvironment env)
        {
            _song = val;
            _artist = val2;
            _user = val3;
            _alb = alb;
            _env = env;
            _p = perm;
        }
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string id)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            Album alb = await _alb.GetById((id));
            if (alb == null)
                return NotFound();
            if (alb.Artists != null)
            {
                if (alb.ArtistList == null)
                    alb.ArtistList = new HashSet<Artist>();
                foreach (string aid in alb.Artists)
                {
                    Artist art = await _artist.GetById((aid));
                    if (art!=null)
                        alb.ArtistList.Add(art);
                }
            }
            if (alb.Collection != null)
            {
                if (alb.SongList == null) alb.SongList = new HashSet<Song>();
                foreach (string sid in alb.Collection)
                {
                    Song s = await _song.GetById((sid));
                    if (s!=null)
                        alb.SongList.Add(s);
                }
            }
            alb.View++;
            _alb.Update((alb.Id), alb);
            var json = JsonConvert.SerializeObject(alb);
            return Ok(json);
        }

        [HttpPost("find")]
        [AllowAnonymous]
        [ResponseCache(Duration = 90, Location =ResponseCacheLocation.Any)]
        public async Task<IActionResult> Find([FromQuery] string q, [FromQuery] int limit=10)
        {
            if (limit <= 0)
                limit = 100;
            HashSet<Album> result = (await ((AlbumDAO)_alb).FindByName(q, limit)).ToHashSet();
            if (result==null || result.Count < limit/2)
            {
                if (q.Contains(" "))
                    result.UnionWith(await ((AlbumDAO)_alb).SmartFindByName(q, limit / 2));
            }
            var json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }

        [HttpPost("list")]
        [Authorize]
        public async Task<IActionResult> GetByPage([FromQuery] int page, [FromQuery] int limit=10)
        {
            if (limit <= 0)
                limit = 20;

            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById(user);
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById(u.Permission);
            if (p == null) return BadRequest();
            if (page <= 0)
                return NotFound();
            if (p.Category<9)
            {
                long count = await ((AlbumDAO)_alb).CountAllByUser(u.Id);
                int maxPage = (int)Math.Ceiling(count / (double)limit);
                if (page > maxPage) return NotFound(maxPage);
                int offset = (page - 1) * limit;
                List<Album> list = await ((AlbumDAO)_alb).GetAllByUser(u.Id, limit, offset);
                var json = JsonConvert.SerializeObject(new { list, maxPage });
                return Ok(json);
            }
            else
            {
                long count = await _alb.CountAll();
                int maxPage = (int)Math.Ceiling(count / (double)limit);
                if (page > maxPage) return NotFound(maxPage);
                int offset = (page - 1) * limit;
                List<Album> list = await _alb.GetAll(limit, offset);
                foreach (var alb in list)
                {
                    if (alb.Artists != null)
                    {
                        if (alb.ArtistList == null)
                            alb.ArtistList = new HashSet<Artist>();
                        foreach (string aid in alb.Artists)
                        {
                            Artist art = await _artist.GetById((aid));
                            if (art != null)
                                alb.ArtistList.Add(art);
                        }
                    }
                    if (alb.Collection != null)
                    {
                        if (alb.SongList == null) alb.SongList = new HashSet<Song>();
                        foreach (string sid in alb.Collection)
                        {
                            Song s = await _song.GetById((sid));
                            if (s != null)
                                alb.SongList.Add(s);
                        }
                    }
                }
                var json = JsonConvert.SerializeObject(new { list, maxPage });

                return Ok(json);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(string name, string desc, string[] artists, string[] songlist, IFormFile file)
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Album album = new Album();
            album.Id = ObjectId.GenerateNewId().ToString();
            album.Name = name;
            album.Desc = desc;
            album.CreatedBy = u.Id;
            album.View = 0;
            album.CreatedOn = DateTime.Now;
            album.Tags = "";
            album.Tags += album.Name.ToLower()+";";
            album.Tags += CharUtil.ConvertToNonUnicode(album.Name.ToLower()) + ";";
            album.Artists = new HashSet<string>();
            #region UploadCover
            if (file != null)
            {
                var fileName = file.FileName;
                if (!file.ContentType.Contains("image"))
                    return new UnsupportedMediaTypeResult();
                var extension = fileName.Substring(fileName.Length - 3);
                var newName = CharUtil.GetRandomName(5, 10) + album.Id + "." + extension;
                var rootPath = _env.WebRootPath + "/_media";
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
                if (!Directory.Exists(rootPath + "/cover"))
                    Directory.CreateDirectory(rootPath + "/cover");
                var filePath = rootPath+  "/cover/" + newName;
                try
                {
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
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                album.AvatarImg = "cover/" + newName;
            }
            #endregion
            if (artists != null && artists.Length>0)
            {
                foreach (string artist in artists)
                {

                    Artist obj = await ((ArtistDAO)_artist).FindByName(artist);
                    if (obj == null)
                    {
                        obj = new Artist()
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            Name = artist,
                            Desc = ""
                        };
                        await _artist.Insert(obj);
                    }
                    album.Artists.Add(obj.Id);
                    album.ArtistList.Add(obj);
                    album.Tags += obj.Name.ToLower() + ";";
                    album.Tags += CharUtil.ConvertToNonUnicode(obj.Name.ToLower()) + ";";
                }
            }
            if (songlist!=null && songlist.Length > 0)
            {
                album.Collection = new HashSet<string>();
                int count = 0;
                foreach (var id in songlist)
                {
                    Song s = await _song.GetById(id);
                    if (s != null)
                    {
                        album.Collection.Add(s.Id);
                        if (count<3)
                            album.Tags += CharUtil.ConvertToNonUnicode(s.Name.Trim().ToLower()) + ";";
                        count++;
                    }
                }
            }
            _alb.Insert(album);
            var json = JsonConvert.SerializeObject(album);
            return Ok(json);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> Edit(string id, string name, string desc, string[] artists = null, string[] songlist=null)
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();

            Album alb = await _alb.GetById((id));
            if (alb == null) return NotFound();
            if (p.Category < 9)
            {
                if (!alb.CreatedBy.Equals(u.Id))
                    return Forbid();
            }

            alb.Name = name;
            alb.Desc = desc;
            alb.Tags = "";
            alb.Tags += alb.Name.ToLower() + ";";
            alb.Tags += CharUtil.ConvertToNonUnicode(alb.Name.ToLower()) + ";";
            if (artists != null && artists.Length > 0)
            {
                alb.Artists = new HashSet<string>();
                foreach (string artist in artists)
                {
                    Artist obj = await ((ArtistDAO)_artist).FindByName(artist);
                    if (obj == null)
                    {
                        obj = new Artist()
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            Name = artist,
                            Desc = ""
                        };
                        await _artist.Insert(obj);
                    }
                    alb.Artists.Add(obj.Id);
                    alb.ArtistList.Add(obj);
                    alb.Tags += obj.Name.ToLower() + ";";
                    alb.Tags += CharUtil.ConvertToNonUnicode(obj.Name.ToLower()) + ";";
                }
            }
            
            if (songlist!=null && songlist.Length>0)
            {
                if (alb.Collection==null)
                    alb.Collection = new HashSet<string>();
                int count = 0;
                foreach (var sid in songlist)
                {
                    if (!alb.Collection.Contains(id))
                    {
                        Song s = await _song.GetById(id);
                        if (s != null)
                        {
                            alb.Collection.Add(s.Id);
                            if (count < 3)
                                alb.Tags += CharUtil.ConvertToNonUnicode(s.Name.Trim().ToLower()) + ";";
                            else break;
                            count++;
                        }
                    }
                }
            }
            _alb.Update((alb.Id), alb);

            var json = JsonConvert.SerializeObject(alb);
            return Ok(json);

        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadImage(string id, IFormFile file)
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();
            Album ab = await _alb.GetById((id));
            if (ab == null) return NotFound();
            if (p.Category <= 8 && !ab.CreatedBy.Equals(u.Id)) return Forbid();
            var fileName = file.FileName;
            if (!file.ContentType.Contains("image"))
                return new UnsupportedMediaTypeResult();
            var extension = fileName.Substring(fileName.Length - 3);
            var newName = CharUtil.GetRandomName(5, 10) + ab.Id + "." + extension;
            var filePath = _env.WebRootPath + "/_media/cover/" + newName;
            try
            {
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
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            if (ab.AvatarImg != null && System.IO.File.Exists(_env.WebRootPath + "/_media/" + ab.AvatarImg))
                System.IO.File.Delete(_env.WebRootPath + "/_media/" + ab.AvatarImg);

            ab.AvatarImg = "cover/" + newName;

            _alb.Update((ab.Id), ab);
            return Ok(JsonConvert.SerializeObject(ab));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {

            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();

            Album alb = await _alb.GetById((id));
            if (alb == null) return NotFound();
            if (p.Category < 9)
            {
                if (!alb.CreatedBy.Equals(u.Id))
                    return Forbid();
            }
            if (alb.IsDeleted==null || alb.IsDeleted == false)
            {
                alb.IsDeleted = true;
                _alb.Update((alb.Id), alb);
            }
            else
            {
                try
                {
                    if (alb.AvatarImg != null && alb.AvatarImg.Length > 0)
                    {
                        var curPath = _env.WebRootPath + "/_media/" + alb.AvatarImg;
                        if (System.IO.File.Exists(curPath))
                        {
                            System.IO.File.Delete(curPath);
                        };
                    }
                    if (alb.BackgroundImg != null && alb.BackgroundImg.Length > 0)
                    {
                        var curPath = _env.WebRootPath + "/_media/" + alb.BackgroundImg;
                        if (System.IO.File.Exists(curPath))
                        {
                            System.IO.File.Delete(curPath);
                        };
                    }
                }
                catch (Exception e)
                {
                }
                _alb.Delete((alb.Id));
            }
            return Ok();
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(string[] ids)
        {

            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return BadRequest();
            HashSet<string> result = new HashSet<string>();
            foreach (var id in ids)
            {
                var alb = await _alb.GetById(id);
                if (alb != null)
                {
                    var skip = false;
                    if (p.Category < 9)
                    {
                        if (!alb.CreatedBy.Equals(u.Id))
                            if (result.Count == 0)
                                return Forbid();
                            else skip = true;
                    }
                    if (!skip)
                    {
                        if (alb.IsDeleted == null || alb.IsDeleted == false)
                        {
                            alb.IsDeleted = true;
                            result.Add(id);
                            _alb.Update((alb.Id), alb);
                        }
                        else
                        {
                            try
                            {
                                if (alb.AvatarImg != null && alb.AvatarImg.Length > 0)
                                {
                                    var curPath = _env.WebRootPath + "/_media/" + alb.AvatarImg;
                                    if (System.IO.File.Exists(curPath))
                                    {
                                        System.IO.File.Delete(curPath);
                                    };
                                }
                                if (alb.BackgroundImg != null && alb.BackgroundImg.Length > 0)
                                {
                                    var curPath = _env.WebRootPath + "/_media/" + alb.BackgroundImg;
                                    if (System.IO.File.Exists(curPath))
                                    {
                                        System.IO.File.Delete(curPath);
                                    };
                                }
                            }
                            catch (Exception e)
                            {
                            }
                            _alb.Delete((alb.Id));
                            result.Add(id);
                        }
                    }
                }
            }
            var json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }
    }
}
