using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using Newtonsoft.Json;
using ATL.AudioData;
using ATL;
using ANWebServices.API;

namespace ANWebServices.Controllers
{

    [Route("api/song")]
    public class SongController : Controller
    {
        private readonly IRepository<Song> _song;
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _p;
        private readonly IRepository<Artist> _artist;
        private readonly IRepository<SongType> _type;
        private readonly IRepository<SongRanked> _ranker;
        private readonly IRepository<RankingCategory> _rnkGrp;
        private readonly IHostingEnvironment _env;
        public SongController(IRepository<Song> val, IRepository<Artist> val2, 
            IRepository<User> val3, IRepository<SongType> type, 
            IRepository<Permission> perm,
            IRepository<SongRanked> ranker,
            IRepository<RankingCategory> rnkGrp,
            IHostingEnvironment env)
        {
            _song = val;
            _artist = val2;
            _user = val3;
            _type = type;
            _env = env;
            _p = perm;
            _ranker = ranker;
            _rnkGrp = rnkGrp;
        }


        [HttpPost("download/{id}")]
        [AllowAnonymous]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> Download(string id)
        {
            try
            {
                Song s = _song.GetById((id)).Result;
                s.HitDownload++;
                _song.Update((s.Id), s);
                string basePath = _env.WebRootPath + "/_media/";
                //===================================================
                FileStream fs = new FileStream(basePath + s.Paths[0].Path, FileMode.Open, FileAccess.Read);
                //BufferedStream bs = new BufferedStream(fs);
                string fileName = s.Name.Trim() + "." + s.Paths[0].Extension;
                fileName = CharUtil.ConvertToNonUnicode(fileName);
                Response.Headers.Add("Content-Disposition", "attachment; filename=" + fileName);

                return File(fs, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        
        [HttpGet("isangit")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetSongISang([FromQuery] int page, [FromQuery] int size)
        {
            List<Song> list = (await _song.Find(x => x.SelfPerformance == 1)).OrderByDescending(x=>x.CreatedOn).ToList();
            int maxPage = (int)Math.Ceiling(list.Count / (double)size);
            int offset = (page - 1) * size;
            if (maxPage == 0) return NoContent();
            if (page > maxPage || page==0) return NotFound();
            var json = JsonConvert.SerializeObject(new
            {
                list = list.GetRange(offset, size),
                currentPage = page,
                maxPage
            });
            return Ok(json);
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration =1800,  Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get()
        {
            List<Song> result = await ((SongDAO)_song).GetAllByView(5, 0);
            foreach (Song s in result)
            {
                s.ArtistList = new HashSet<Artist>();
                foreach (string id in s.Artists)
                    s.ArtistList.Add(await _artist.GetById((id)));
            }
            var json = JsonConvert.SerializeObject(result);
            return new OkObjectResult(json);
        }


        [HttpGet("detail/{songid}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetDetail(string songid)
        {
            Song s = await _song.GetById((songid));

            var filePath = _env.WebRootPath + "/_media/" + s.Paths[0].Path;
            if (s == null || s.Approved == 0) return NotFound();
            var f = new Track(filePath);
            if (s.Artists!=null && s.Artists.Count>0)
            {
                s.ArtistList = new HashSet<Artist>();
                foreach (string id in s.Artists)
                    s.ArtistList.Add(await _artist.GetById((id)));
            }
            s.Duration = f.Duration;
            return Ok(JsonConvert.SerializeObject(s));
        }

        [HttpPost("find")]
        [AllowAnonymous]
        [ResponseCache(Location =ResponseCacheLocation.Any, Duration = 60 )]
        public async Task<IActionResult> Find([FromQuery] string q, [FromQuery] int limit=5)
        {
            if (string.IsNullOrEmpty(q)) return NoContent();
            if (limit==null || limit <= 0) limit = 5;
            var keyword = q.ToLower().Trim();
            var threshold = keyword.Length * (2 / 3);
            HashSet<Song> result = new HashSet<Song>();
            foreach (var song in (await _song.Find(x => !x.IsDeleted && x.Tags.Contains(keyword), limit, 0)))
                result.Add(song);
            if (result.Count<limit && keyword.Contains(" "))
            {
                string[] words = keyword.Split(" ");
                int count = limit / 2;
                if (count <= 1) count = 2;
                foreach (var w in words)
                {
                    foreach (var song in (await _song.Find(x => !x.IsDeleted && x.Tags.Contains(w), limit, 0)))
                        result.Add(song);
                    if (result.Count > limit) break;
                }
            }
            if (result.Count <= 0) return NoContent();
            var json = JsonConvert.SerializeObject(result);
            return Ok(json);
        }


        [HttpGet("play/{id}")]
        [AllowAnonymous]
        [EnableCors("ServeablePages")]
        public async Task<IActionResult> GetSongToPlay(string id)
        {
            #region Prevent Outsite Request
            StringValues reff=new StringValues();
            HttpContext.Request.Headers.TryGetValue("Referer", out reff);

            if (reff.Count <= 0)
                return NotFound();
            #endregion
            try
            {
                Song s = await _song.GetById((id));
                if (s == null) return NotFound();
                string basePath = _env.WebRootPath + "/_media/";
                //===================================================
                FileStream fs;
                try
                {
                    fs = new FileStream(basePath + s.Paths[0].Path, 
                        FileMode.Open, FileAccess.Read, 
                        FileShare.Read, 4069, FileOptions.Asynchronous);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
                long fSize = fs.Length;
                long startbyte = 0;
                long endbyte = fSize - 1;
                long maxBuffer = 1048;
                int statusCode = 200; //OK
                var rangeRequest = Request.Headers["Range"].ToString();
                if ((rangeRequest != null && rangeRequest != ""))
                {
                    //Get the actual byte range from the range header string, and set the starting byte.
                    string[] range = Request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                    startbyte = Convert.ToInt64(range[1]);
                    if (range.Length > 2 && range[2] != "")
                    {
                        endbyte = Convert.ToInt64(range[2]);
                        if (endbyte - startbyte > maxBuffer)
                        {
                            endbyte = startbyte + maxBuffer;
                            if (endbyte > fSize - 1)
                                endbyte = fSize - 1;
                        }
                    }
                    //If the start byte is not equal to zero, that means the user is requesting partial content.
                    if (startbyte != 0 || endbyte != fSize - 1 || range.Length > 2 && range[2] == "")
                    { statusCode = 206; }//Set the status code of the response to 206 (Partial Content) and add a content range header.                                    

                }
                long desSize = endbyte - startbyte + 1;
                //Headers

                Response.StatusCode = statusCode;
                // Remove mismatch headers
                HttpContext.Request.Headers.Remove("If-Modified-Since");
                HttpContext.Request.Headers.Remove("If-None-Match");
                Response.ContentType = "audio/" + s.Paths[0].Extension;
                Response.Headers.Add("Content-Accept", Response.ContentType);
                Response.Headers.Add("Content-Length", desSize.ToString());
                Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
                Response.Headers.Add("Accept-Ranges", "bytes");
                Response.Headers.Remove("Cache-Control");
                //Data
                fs.Seek(startbyte, SeekOrigin.Begin);
                if (startbyte == 0 && s.Approved > 0)
                {
                    s.View++;
                    _song.Update((s.Id), s);
                }
                return new FileStreamResult(fs, Response.ContentType);
            }
            catch (InvalidOperationException)
            {
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        private Song CheckTags(Song s, bool skip = false)
        {

            if (s.Tags == null || s.Tags.Length <= 0 || skip)
            {
                s.Tags += s.Name.ToLower() + ";";
                if (s.Name.Contains(" ")) s.Tags += s.Name.ToLower().Replace(" ", "") + ";";
                var nouni = CharUtil.ConvertToNonUnicode(s.Name.ToLower());
                if (nouni != s.Name.ToLower())
                {
                    s.Tags += nouni + ";";
                    if (s.Name.Contains(" ")) s.Tags += s.Name.ToLower().Replace(" ", "") + ";";
                }
                foreach (string tid in s.Artists)
                {
                    Artist a = _artist.GetById((tid)).Result;
                    s.Tags += a.Name.ToLower() + ";";
                    if (a.Name.Contains(" ")) s.Tags += a.Name.ToLower().Replace(" ", "") + ";";
                }
                SongType t = ((TypeDAO)_type).GetBySubId(s.SubType).Result;
                if (t != null)
                {
                    s.Tags += t.Text.ToLower() + ";";
                    s.Tags += CharUtil.ConvertToNonUnicode(t.Text.ToLower()) + ";";
                    if (t.Text.Contains(" ")) s.Tags += CharUtil.ConvertToNonUnicode(t.Text.ToLower()).Replace(" ", "") + ";";
                    SongSubType st = ((TypeDAO)_type).GetSubType(t, s.SubType);
                    s.Tags += st.Text.ToLower() + ";";
                    s.Tags += CharUtil.ConvertToNonUnicode(st.Text.ToLower()) + ";";
                    if (st.Text.Contains(" ")) s.Tags += CharUtil.ConvertToNonUnicode(st.Text.ToLower()).Replace(" ", "") + ";";
                }
            }
            return s;
        }


        private async Task<bool> ModifySong(string filePath, Song s)
        {
            if (System.IO.File.Exists(filePath))
            {
                List<string> artNames = new List<string>();
                if (s.ArtistList != null)
                    foreach (Artist a in s.ArtistList)
                        artNames.Add(a.Name);
                try
                {
                    var f = new Track(filePath);
                    f.Album = "Amnhac.com";
                    f.AlbumArtist = "";
                    foreach (string artName in artNames)
                        f.AlbumArtist += artName + ";";
                    f.Comment = "This song was uploaded to Amnhac.com";
                    f.Copyright = "Amnhac.com";
                    f.Artist = f.AlbumArtist;
                    SongType st = await ((TypeDAO)_type).GetBySubId(s.SubType);
                    if (st != null)
                    {
                        SongSubType sst = ((TypeDAO)_type).GetSubType(st, s.SubType);
                        f.Genre = CharUtil.ConvertToNonUnicode(sst.Text);
                    }
                    f.Title = CharUtil.ConvertToNonUnicode(s.Name);

                    f.Save();
                    
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return false;
        }
        
        [HttpPut("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(
            string id,
            string name,
            string subtype,
            IFormFile file,
            string self="0",
            string[] artists=null
        )
        {
            if (!ModelState.IsValid)
                return BadRequest();
            try
            {
                string user = HttpContext.User.FindFirst("id").Value;
                User u = await _user.GetById((user));
                if (u == null) return Unauthorized();
                Permission p = await _p.GetById((u.Permission));
                if (id == null)
                {
                    Song s = new Song()
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Name = name,
                        SubType = subtype,
                        Paths = new List<SongPath>(),
                        Artists = new List<string>()
                    };
                    var fileName = file.FileName;
                    if (!file.ContentType.Contains("audio"))
                        return new UnsupportedMediaTypeResult();
                    var extension = fileName.Substring(fileName.Length - 3);
                    var newName = CharUtil.GetRandomName(5, 10) + s.Id.ToString() + "." + extension;
                    var rootFolder = _env.WebRootPath + "/_media";
                    var filePath = rootFolder+"/audio/" + newName;

                    if (!Directory.Exists(rootFolder))
                        Directory.CreateDirectory(rootFolder);
                    if (!Directory.Exists(rootFolder + "/audio"))
                        Directory.CreateDirectory(rootFolder + "/audio");

                    using (var fs = file.OpenReadStream())
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        long bufferSize = 1000000;
                        if (file.ContentType.Contains("wav"))
                        {
                            AudioConverter.WavToMp3(bs, filePath + ".mp3");
                        }
                        else
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
                    if (user != null) s.UploadedBy = user;
                    SongPath item = new SongPath()
                    {
                        Path = "audio/" + newName,
                        Prefix = 1,
                        Extension = extension
                    };
                    s.Paths.Add(item);
                    if (s.ArtistList == null)
                        s.ArtistList = new HashSet<Artist>();
                    string artList = "";
                    if (artists != null)
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
                                obj.Alphabet = CharUtil.ConvertToNonUnicode(obj.Name.Substring(0, 1)).ToUpper();
                                 _artist.Insert(obj);
                            }
                            else
                            {
                                if (obj.IsDeleted == true)
                                {
                                    obj.IsDeleted = false;
                                    _artist.Update((obj.Id), obj);
                                }
                            }
                            s.Artists.Add(obj.Id.ToString());
                            s.ArtistList.Add(obj);
                            artList += obj.Name;
                        }

                    ModifySong(filePath, s);
                    s.SelfPerformance = int.Parse(self);

                    s = CheckTags(s);

                    _song.Insert(s);
                    return new OkObjectResult(JsonConvert.SerializeObject(s)); //PartialView("~/Views/Shared/_SongList.cshtml");
                }
                else
                {
                    Song s = _song.GetById((id)).Result;
                    if (p.Category <= 8 && !s.UploadedBy.Equals(u.Id.ToString()))
                        return Forbid();
                    s.Name = name;
                    s.SubType = subtype;
                    if (s.ArtistList == null)
                        s.ArtistList = new HashSet<Artist>();
                    if (artists != null)
                        for (int i = 0; i < artists.Length; i++)
                        {
                            Artist a = await ((ArtistDAO)_artist).FindByName(artists[i]);
                            if (a == null)
                            {
                                a = new Artist()
                                {
                                    Id = ObjectId.GenerateNewId().ToString(),
                                    Name = artists[i]
                                };
                                a.Alphabet = CharUtil.ConvertToNonUnicode(a.Name.Substring(0, 1)).ToUpper();
                                _artist.Insert(a);
                            }
                            if (s.Artists.IndexOf(a.Id.ToString()) < 0 && a.Id != null)
                                s.Artists.Add(a.Id.ToString());
                            s.ArtistList.Add(a);
                        }
                    s.SelfPerformance = int.Parse(self);

                    s = CheckTags(s, true);

                    _song.Update((s.Id), s);
                    return new OkObjectResult(JsonConvert.SerializeObject(s));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message+":"+e.Source);
            }
        }


        [HttpPost("page/{max}/{page}")]
        [Authorize]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetPage(int max, int page,[FromBody] string keywords)
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            try
            {
                List<Song> result;
                Permission p = await _p.GetById((u.Permission));
                int level = p.Category;
                long totalCount = 0;
                if (level >= 9)
                {
                    if (keywords != null && keywords.Length>0)
                        totalCount = await ((SongDAO)_song).CountBySmartFind(keywords, 0, 10);
                    else
                        totalCount = await _song.CountAll();
                    if (max * (page - 1) > totalCount) { return NotFound(); }
                    if (keywords != null && keywords.Length > 0)
                        result = await ((SongDAO)_song).SmartFinder(keywords, 0, 10, max, max * (page - 1));
                    else
                        result = await _song.GetAll(max, max * (page - 1));
                }
                else
                {
                    if (keywords != null && keywords.Length > 0)
                        totalCount = await ((SongDAO)_song).CountBySmartFind(keywords, 0, 10, u.Id.ToString());
                    else
                        totalCount =await ((SongDAO)_song).CountByUser(userId);
                    if (max * (page - 1) > totalCount) { return NotFound(); }
                    if (keywords != null && keywords.Length > 0)
                        result = await ((SongDAO)_song).SmartFinder(keywords, 0, 10, max, max * (page - 1), u.Id.ToString());
                    else
                        result = await ((SongDAO)_song).GetAllWithFilter(0,10, max, max * (page - 1),u.Id.ToString());
                }
                HashSet<Artist> artistList = new HashSet<Artist>();
                HashSet<string> foundArtist = new HashSet<string>();
                foreach (var item in result)
                {
                    if (item.UploadedBy!=null)
                    {
                        User temp = await _user.GetById((item.UploadedBy));
                        if (temp != null)
                        {
                            temp.ClearSentitiveData();
                            item.UploadedUser = temp;
                        }
                    }
                    for (int j = 0; j < item.Artists.Count; j++)
                    {
                        if (!foundArtist.Contains(item.Artists[j]))
                        {
                            Artist art = _artist.GetById((item.Artists[j])).Result;
                            foundArtist.Add(item.Artists[j]);
                            artistList.Add(art);
                        }
                    }
                }
                var rjson=new { list = result, artistList , totalpage = Math.Ceiling((double)totalCount / max) };
                return new OkObjectResult(JsonConvert.SerializeObject(rjson));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("pagefilter/{toggle}/{max}/{page}")]
        [Authorize]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetFilterPage(int toggle, int max, int page, [FromBody] string keywords)
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            try
            {
                List<Song> result;
                Permission p = await _p.GetById((u.Permission));
                int level = p.Category;
                long totalCount = 0;
                if (level >= 9)
                {
                    if (keywords != null && keywords.Length > 0)
                        totalCount = await ((SongDAO)_song).CountBySmartFind(keywords, toggle, toggle * 5);
                    else
                        totalCount = await ((SongDAO)_song).CountWithFilter(toggle,toggle * 5);
                    if (max * (page - 1) > totalCount) { page = (int)Math.Floor(totalCount / (decimal)max); }
                    if (page < 1) page = 1;
                    if (keywords!= null && keywords.Length > 0)
                        result=await ((SongDAO)_song).SmartFinder(keywords,toggle,toggle*5, max, max * (page - 1));
                    else
                        result = await ((SongDAO)_song).GetAllWithFilter(toggle,toggle * 5, max, max * (page - 1));
                }
                else
                {
                    if (keywords != null && keywords.Length > 0)
                        totalCount = await ((SongDAO)_song).CountBySmartFind(keywords,toggle, toggle * 5, u.Id.ToString());
                    else
                        totalCount = await ((SongDAO)_song).CountWithFilter(toggle,toggle * 5, u.Id.ToString());
                    if (max * (page - 1) > totalCount) { page = (int)Math.Floor(totalCount / (decimal)max); }
                    if (page < 1) page = 1;
                    if (keywords != null && keywords.Length > 0)
                        result = await ((SongDAO)_song).SmartFinder(keywords, toggle, toggle * 5, max, max * (page - 1), u.Id.ToString());
                    else
                        result = await ((SongDAO)_song).GetAllWithFilter(toggle,toggle * 5, max, max * (page - 1), u.Id.ToString());
                }

                HashSet<Artist> artistList = new HashSet<Artist>();
                HashSet<string> foundArtist = new HashSet<string>();
                foreach (var item in result)
                {
                    if (item.UploadedBy != null)
                    {
                        User temp = await _user.GetById((item.UploadedBy));
                        if (temp != null)
                        {
                            temp.ClearSentitiveData();
                            item.UploadedUser = temp;
                        }
                    }
                    for (int j = 0; j < item.Artists.Count; j++)
                    {
                        if (!foundArtist.Contains(item.Artists[j]))
                        {
                            Artist art = _artist.GetById((item.Artists[j])).Result;
                            foundArtist.Add(item.Artists[j]);
                            artistList.Add(art);
                        }
                    }
                }
                var rjson = new { list = result, artistList, totalpage = Math.Ceiling((double)totalCount / max),pageResult=page };
                return new OkObjectResult(JsonConvert.SerializeObject(rjson));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        public class SongAllowPackage
        {
            public string[] songs;
            public int allow;
        }

        [HttpPut("favorite/{songid}")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(string songid)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Song s = await _song.GetById((songid));
            if (s != null)
            {
                string id = s.Id.ToString();
                if (u.SongFavorite.Contains(id))
                    u.SongFavorite.Remove(id);
                else
                    u.SongFavorite.Add(id);
                _user.Update((u.Id), u);

                var json = JsonConvert.SerializeObject(u.SongFavorite);
                return Ok(json);
            }
            else return BadRequest();
        }


        [HttpGet("favorite")]
        [Authorize]
        public async Task<IActionResult> GetFavorite()
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();

            if (u.SongFavorite != null && u.SongFavorite.Count > 0)
            {
                List<Song> result = new List<Song>();
                foreach (string id in u.SongFavorite) {
                    Song s = await _song.GetById((id));
                    if (s != null)
                    {
                        s.ArtistList = new HashSet<Artist>();
                        foreach (string aid in s.Artists)
                            s.ArtistList.Add(await _artist.GetById((aid)));
                        if (s != null)
                            result.Add(s);
                    }
                }

                var json = JsonConvert.SerializeObject(result);
                return Ok(json);
            }
            else return new EmptyResult();
        }

        [HttpPut("approve")]
        [Authorize(Roles = "10,9,8")]
        public async Task<IActionResult> AllowSong([FromBody] SongAllowPackage model)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p==null || p.Category <= 6) return Forbid();
            try
            {
                foreach (string id in model.songs)
                {
                    Song s =await _song.GetById((id));
                    if (s != null)
                    {
                        s.Approved = model.allow;
                        _song.Update((s.Id), s);
                        var ranker = await _ranker.CountIf(x => x.SongId == s.Id);
                        if (ranker==0)
                        {
                            var grpCount = await _rnkGrp.CountAll();
                            var rank = new SongRanked() {
                                SongId=s.Id,
                                LastUpdate=DateTime.Now.AddMonths(-1),
                                ValidFrom=DateTime.Now,
                                ValidTo=DateTime.Now.AddMonths(3),
                                Priority=0
                            };
                            if (s.Approved > 1)
                                rank.Priority = 1;
                            if (grpCount > 0)
                            {
                                var gr = (await _rnkGrp.Find(x => x.TypeList.Any(t => t == s.SubType))).FirstOrDefault();
                                if (gr != null)
                                {
                                    rank.Category = gr.Id;
                                    _ranker.Insert(rank);
                                }
                            }
                            else
                                _ranker.Insert(rank);
                        }
                    }

                }
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            try
            {
                Permission p = await _p.GetById((u.Permission));
                Song song = await _song.GetById((id));
                if (p.Category <= 7)
                {
                    if (song.UploadedBy.Equals(u.Id.ToString()))
                    {
                        if (p.Category <= 4)
                        {
                            if (song.View < 5000)
                                return new StatusCodeResult(406);
                            else
                            {
                                DeleteFile(song);
                                await _song.Delete((id));
                                return Ok();
                            }
                        }
                        else
                        {
                            DeleteFile(song);
                            await _song.Delete((id));
                            return Ok();
                        }
                    }
                    else return new ForbidResult();
                }
                else
                {
                    DeleteFile(song);
                    await _song.Delete((id));
                    return Ok();
                }
            }
            catch (Exception e) { return BadRequest(e.Message); }
        }


        [HttpPut("delete")]
        [Authorize]
        public async Task<IActionResult> BatchDelete([FromBody] string[] id)
        {
            if (!ModelState.IsValid) return BadRequest();
            string userId = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userId));
            if (u == null) return Unauthorized();
            if (id == null) return BadRequest();
            try
            {
                Permission p = await _p.GetById((u.Permission));
                if (p.Category <= 7)
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        Song song = await _song.GetById((id[i]));
                        if (song.UploadedBy.Equals(u.Id.ToString()))
                        {
                            if (p.Category <= 4)
                            {
                                if (song.View < 5000)
                                {

                                }
                                else
                                {
                                    DeleteFile(song);
                                    await _song.Delete((id[i]));
                                }
                            }
                            else
                            {
                                DeleteFile(song);
                                await _song.Delete((id[i]));
                            }
                        }
                        else return new ForbidResult();
                    }
                    return Ok();
                }
                else
                {
                    for (int i = 0; i < id.Length; i++)
                    {
                        Song song = await _song.GetById((id[i]));
                        DeleteFile(song);
                        await _song.Delete((id[i]));
                    }
                    return Ok();
                }
            }
            catch (Exception e) { return BadRequest(e.Message); }
        }

        private async void DeleteFile(Song s)
        {
            foreach (SongPath path in s.Paths)
            {
                var filePath = _env.WebRootPath + "/_media/" + path.Path;
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
        }
    }
}