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
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/video")]
    public class VideoController : Controller
    {
        private readonly IHostingEnvironment _env;
        private readonly IRepository<Video> _video;
        private readonly IRepository<Permission> _p;
        private readonly IRepository<User> _usr;
        private readonly IRepository<Artist> _art;
        private readonly IRepository<SongType> _type;
        public VideoController(IHostingEnvironment env, IRepository<Video> video,
            IRepository<Permission> p, IRepository<User> user, IRepository<Artist> art,
            IRepository<SongType> type
            )
        {
            _env = env;
            _video=video;
            _p = p;
            _usr = user;
            _art = art;
            _type = type;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> Upload(string name, string[] artists, string type, IFormFile file)
        {
            if (!ModelState.IsValid) return BadRequest();

            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _usr.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return Forbid();
                Video s = new Video()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = name,
                    Type = type,
                    Path = "",
                    Approve=0,
                    Artists = new HashSet<string>()
                };
            string filePath = "";
            string ext = "";
            if (!UploadFile(file, s.Id, "video",ref filePath, ref ext)) return new UnsupportedMediaTypeResult();
            if (user != null) s.UploadBy = user;
            //SongPath item = new SongPath()
            //{
            //    Path = "audio/" + newName,
            //    Prefix = 1,
            //    Extension = extension
            //};
            s.Extension = ext;
            s.Path = filePath;
            
                if (s.ArtistList == null)
                    s.ArtistList = new List<Artist>();
                string artList = "";
                if (artists != null)
                    foreach (string artist in artists)
                    {
                        Artist obj = await ((ArtistDAO)_art).FindByName(artist);
                        if (obj == null)
                        {
                            obj = new Artist()
                            {
                                Id = ObjectId.GenerateNewId().ToString(),
                                Name = artist,
                                Desc = ""
                            };
                            obj.Alphabet = CharUtil.ConvertToNonUnicode(obj.Name.Substring(0, 1)).ToUpper();
                            _art.Insert(obj);
                        }
                        else
                        {
                            if (obj.IsDeleted == true)
                            {
                                obj.IsDeleted = false;
                                _art.Update((obj.Id), obj);
                            }
                        }
                        s.Artists.Add(obj.Id.ToString());
                        s.ArtistList.Add(obj);
                        artList += obj.Name;
                    }
                

                s = CheckTags(s);

                _video.Insert(s);
                return new OkObjectResult(JsonConvert.SerializeObject(s)); //PartialView("~/Views/Shared/_SongList.cshtml");

            }
            


        public bool UploadFile(IFormFile file, string prefix, string folder, ref string outputPath, ref string ext)
        {

            var fileName = file.FileName;
            if (!file.ContentType.Contains("video"))
                return false;
            var extension = fileName.Substring(fileName.LastIndexOf(".") + 1);
            var newName = CharUtil.GetRandomName(5, 10) + prefix + "." + extension;
            var rootFolder = _env.WebRootPath + "/_media";
            var filePath = rootFolder + "/"+folder+"/" + newName;

            if (!Directory.Exists(rootFolder))
                Directory.CreateDirectory(rootFolder);
            if (!Directory.Exists(rootFolder + "/"+folder))
                Directory.CreateDirectory(rootFolder + "/"+folder);

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
            outputPath = folder + "/" + newName;
            ext = extension;
            return true;
        }
        public Video CheckTags(Video v, bool skip=false)
        {

            if (v.Tags == null || v.Tags.Length <= 0 || skip)
            {
                v.Tags += v.Name.ToLower() + ";";
                if (v.Name.Contains(" ")) v.Tags += v.Name.ToLower().Replace(" ", "") + ";";
                var nouni = CharUtil.ConvertToNonUnicode(v.Name.ToLower());
                if (nouni != v.Name.ToLower())
                {
                    v.Tags += nouni + ";";
                    if (v.Name.Contains(" ")) v.Tags += v.Name.ToLower().Replace(" ", "") + ";";
                }
                foreach (string tid in v.Artists)
                {
                    Artist a = _art.GetById((tid)).Result;
                    v.Tags += a.Name.ToLower() + ";";
                    if (a.Name.Contains(" ")) v.Tags += a.Name.ToLower().Replace(" ", "") + ";";
                }
                SongType t = ((TypeDAO)_type).GetBySubId(v.Type).Result;
                if (t != null)
                {
                    v.Tags += t.Text.ToLower() + ";";
                    v.Tags += CharUtil.ConvertToNonUnicode(t.Text.ToLower()) + ";";
                    if (t.Text.Contains(" ")) v.Tags += CharUtil.ConvertToNonUnicode(t.Text.ToLower()).Replace(" ", "") + ";";
                    SongSubType st = ((TypeDAO)_type).GetSubType(t, v.Type);
                    v.Tags += st.Text.ToLower() + ";";
                    v.Tags += CharUtil.ConvertToNonUnicode(st.Text.ToLower()) + ";";
                    if (st.Text.Contains(" ")) v.Tags += CharUtil.ConvertToNonUnicode(st.Text.ToLower()).Replace(" ", "") + ";";
                }
            }
            return v;
        }

        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 10, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Get([FromQuery] int size)
        {
            var list = await ((VideoDAO)_video).GetRandom(size);
            return Ok(JsonConvert.SerializeObject(new { list }));
        }


        [HttpGet("manage")]
        [Authorize]
        public async Task<IActionResult> GetToManage([FromQuery] int page, [FromQuery] int size, [FromQuery] string q="")
        {
            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _usr.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null) return Forbid();
            long count = 0;
            List<Video> result = new List<Video>();
            if (p.Category < 9)
            {
                if (string.IsNullOrEmpty(q))
                    count = await _video.CountIf(x => x.UploadBy.Equals(user) );
                else
                count = await _video.CountIf(x => x.UploadBy.Equals(user) && x.Tags.ToLower().Contains(q.ToLower()));
                var offset = (page - 1) * size;
                if (offset>=0 && count>0 && count >= offset)
                    if (string.IsNullOrEmpty(q))
                        result = await _video.Find(x => x.UploadBy.Equals(user) , size, offset);
                    else
                        result = await _video.Find(x => x.UploadBy.Equals(user) && x.Tags.ToLower().Contains(q.ToLower()), size, offset);
            }
            else
            {
                if (string.IsNullOrEmpty(q))
                    count = await _video.CountAll();
                else
                    count = await _video.CountIf(x=>(string.IsNullOrEmpty(q) || x.Tags.ToLower().Contains(q.ToLower())));
                var offset = (page - 1) * size;
                if (offset >= 0 && count > 0 && count >= offset)
                    if (string.IsNullOrEmpty(q))
                        result = await _video.Find(_=>true, size, offset);
                    else
                        result = await _video.Find(x=>(x.Tags.ToLower().Contains(q.ToLower())),size, offset);
                foreach (var vid in result)
                {
                    if (!string.IsNullOrEmpty(vid.UploadBy))
                    {
                        vid.Uploader = await _usr.GetById(vid.UploadBy);
                        if (vid.Uploader != null)
                            vid.Uploader.ClearSentitiveData();
                    }
                }
            }
            var maxPage =  Math.Ceiling(count / (double)size);
            if (maxPage == 0 || page > maxPage || page <= 0) return NoContent();
            return Ok(JsonConvert.SerializeObject(new { list=result, maxPage }));
        }

        public class VideoApproveRequest
        {
            public string[] id { get; set; }
            public int approve { get; set; }
        }
        [HttpPut("approve")]
        [Authorize(Roles ="9,10")]
        public async Task<IActionResult> Approve([FromBody] VideoApproveRequest req)
        {
            if (!ModelState.IsValid) return BadRequest();

            string user = HttpContext.User.FindFirst("id").Value;
            User u = await _usr.GetById((user));
            if (u == null) return Unauthorized();
            Permission p = await _p.GetById((u.Permission));
            if (p == null || p.Category < 9) return Forbid();
            Video target;
            HashSet<Video> updated = new HashSet<Video>();
            foreach (string id in req.id)
            {
                target = await _video.GetById(id);
                if (target!=null)
                {
                    target.Approve = req.approve;
                    _video.Update(id, target);
                    updated.Add(target);
                }
            }

            if (updated.Count > 0) return Ok(JsonConvert.SerializeObject(updated));
            return StatusCode(304); // NOT MODIFIED
        }

        [HttpGet("play/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Play(string id)
        {
            #region Prevent Outsite Request
            StringValues reff = new StringValues();
            HttpContext.Request.Headers.TryGetValue("Referer", out reff);

            if (reff.Count <= 0)
                return NotFound();
            #endregion
            try
            {
                Video s = await _video.GetById((id));
                if (s == null) return NotFound();
                string basePath = _env.WebRootPath + "/_media/";
                //===================================================
                FileStream fs;
                try
                {
                    fs = new FileStream(basePath + s.Path,
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
                Response.ContentType = "video/" + s.Extension;
                Response.Headers.Add("Content-Accept", Response.ContentType);
                Response.Headers.Add("Content-Length", desSize.ToString());
                Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
                Response.Headers.Add("Accept-Ranges", "bytes");
                Response.Headers.Remove("Cache-Control");
                //Data
                fs.Seek(startbyte, SeekOrigin.Begin);
                if (startbyte == 0 && s.Approve > 0)
                {
                    s.View++;
                    _video.Update((s.Id), s);
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

        [HttpPost("{id}")]
        [AllowAnonymous]
        [ResponseCache(Duration =30, Location =ResponseCacheLocation.Any )]
        public async Task<IActionResult> GetById(string id)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = await _video.GetById(id);
            return Ok(JsonConvert.SerializeObject(result));
        }
    }


}
