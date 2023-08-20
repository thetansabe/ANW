using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Amnhac.Api;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.API;
using ANWebServices.Interfaces;
using ANWebServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace ANWebServices.Controllers
{
    [Produces("application/json")]
    [Route("api/user")]
    [EnableCors("ServeablePages")]
    public class UserController : Controller
    {
        private readonly IRepository<User> _user;
        private readonly IRepository<Permission> _perm;
        private readonly IRepository<SongType> _type;
        private readonly IHubContext<SignalRService> _signalR;
        private readonly IJwtFactory _jwt;
        private readonly IHostingEnvironment _env;


        private readonly JsonSerializerSettings _serializerSettings;
        public UserController(IRepository<User> val, 
            IRepository<Permission> val2, 
            IRepository<SongType> val3, 
            IJwtFactory jwtOptions,
            IHubContext<SignalRService> signalR,
            IHostingEnvironment env
            )
        {
            _jwt = jwtOptions;
            _user = val;
            _perm = val2;
            _type = val3;
            _signalR = signalR;
            _env = env;
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
        }

        //[HttpPost("permission/get/{id}")]
        //[Authorize]
        //[ResponseCache(Duration = 60 * 60 * 24 * 7, Location = ResponseCacheLocation.Any)]
        //public ActionResult GetPermission(string id)
        //{
        //    try
        //    {
        //        return Json(_perm.GetById((id)).Result);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { val = e.Message });
        //    }
        //}
        
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] bool rememberMe=false)
        {
            string authHeader = HttpContext.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith(JwtFactory.AuthorizationHeader))
            {
                try
                {
                    string content = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    string[] decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(content)).Split(":", 2);

                    string username = decodedContent[0];
                    string password = decodedContent[1];
                    var identity = await GetClaimsIdentity(username, password);
                    if (identity == null) return new UnauthorizedResult();
                    string token = "";
                    if (rememberMe)
                    {
                        token = await _jwt.GetToken(username, identity, 0, 24 * 7);
                    }
                    else token = await _jwt.GetToken(username, identity);
                    User u = await ((UserDAO)_user).FindByUsername(username);
                    u.ClearSentitiveData();
                    var response = new
                    {
                        userid = identity.Claims.Single(c => c.Type == "id").Value,
                        profile=u,
                        access_token = token
                    };

                    var json = JsonConvert.SerializeObject(response, _serializerSettings);

                    return new OkObjectResult(json);
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }
            else return BadRequest();
        }

        [HttpPost("getprofile")]
        [AllowAnonymous]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> GetProfile([FromBody] string userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                User u = await _user.GetById((userId));
                if (u != null)
                {
                    u.ClearSentitiveData();
                    var json = JsonConvert.SerializeObject(u);
                    return new OkObjectResult(json);
                }
            }
            catch (Exception){ }
            return BadRequest();
        }

        [HttpPost("register")]
        [ResponseCache(NoStore =true)]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] User model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                long userCount = ((UserDAO)_user).CountUsername(model.Username).Result;
                long emailCount = ((UserDAO)_user).CountEmail(model.Email).Result;
                if (userCount == 0 && emailCount == 0 && model.Username != null && model.Email != null)
                {
                    Permission p = (await ((PermissionDAO)_perm).FindByCategory(0)).FirstOrDefault();
                    if (p == null)
                    {
                        Permission temp = new Permission()
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            Category = 0,
                            Name = "Free User",
                            Desc = "Free user with no validated email.",
                            Price = 0
                        };
                        await _perm.Insert(temp);
                        p = temp;
                    }
                    string key = Startup.secret_key;
                    string encodedPass = Convert.ToBase64String(Encoding.UTF8.GetBytes(model.Username + ":" + model.Password));
                    User u = new User()
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Username = model.Username,
                        Email = model.Email,
                        DisplayName = model.Username,
                        FullName = model.FullName,
                        Phone = model.Phone,
                        Idcard = model.Idcard,
                        Permission = p.Id.ToString()
                    };
                    var password = CharUtil.AesEnc(CharUtil.PasswordHash(encodedPass, key));
                    u.Password = password;
                    await _user.Insert(u);
                    u.Password = null;
                    var json = JsonConvert.SerializeObject(u, _serializerSettings);
                    return new OkObjectResult(json);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(new { emailCount, userCount }, _serializerSettings);
                    return new BadRequestObjectResult(json);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            
        }

        [HttpPut("save")]
        [Authorize]
        public async Task<IActionResult> Edit([FromBody] User user)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _perm.GetById((u.Permission));
            if (u.Id.Equals(user.Id))
            {
                if (user.Permission != null)
                {
                    Permission newPermission = await _perm.GetById((user.Permission));
                    if (newPermission == null) return NotFound();
                    if (newPermission.Category >= 8)
                        return BadRequest();
                    u.Permission = newPermission.Id;
                }
                u.AvatarImg = user.AvatarImg;
                u.BackgroundImg = user.BackgroundImg;
                u.DateOfBirth = user.DateOfBirth;
                u.Desc = user.Desc;
                u.DisplayName = user.DisplayName;
                u.FullName = user.FullName;
                u.Idcard = user.Idcard;
                u.Phone = user.Phone;

                _user.Update((u.Id), u);
                return Ok();
            }
            else if (p.Category >= 9)
            {
                if (user.Permission != null)
                {
                    Permission newPermission = await _perm.GetById((user.Permission));
                    if (newPermission == null) return NotFound();
                    if (p.Category != 10 && newPermission.Category >= p.Category)
                        return BadRequest();
                    u.Permission = newPermission.Id;
                }
                u.AvatarImg = user.AvatarImg;
                u.BackgroundImg = user.BackgroundImg;
                u.DateOfBirth = user.DateOfBirth;
                u.Desc = user.Desc;
                u.DisplayName = user.DisplayName;
                u.FullName = user.FullName;
                u.Idcard = user.Idcard;
                u.Phone = user.Phone;

                _user.Update((u.Id), u);
                return Ok();
            }
            return Forbid();
        }



        [HttpPost("profile")]
        [Authorize]
        public async Task<IActionResult> GetMyInfo()
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            u.Password = "";
            return Ok(JsonConvert.SerializeObject(u));
        }


        public class ChangePasswordRequest
        {
            public string oldpass { get; set; }
            public string newpass { get; set; }
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest model)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            Permission p = await _perm.GetById((u.Permission));
            string oldpass = Encoding.UTF8.GetString(Convert.FromBase64String(model.oldpass));
            string newpass = Encoding.UTF8.GetString(Convert.FromBase64String(model.newpass));
            if (u.Password.Equals(CharUtil.AesEnc(oldpass)))
            {
                u.Password = CharUtil.AesEnc(newpass);
                return Ok();
            }
            else return BadRequest();
        }

        [HttpPost("upload/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            if (file != null)
            {

                if (u.AvatarImg!=null && u.AvatarImg.Length > 0)
                {
                    var curPath = _env.WebRootPath + "/resources/" +u.AvatarImg;
                    if (System.IO.File.Exists(curPath)){
                        System.IO.File.Delete(curPath);
                    };
                }

                var fileName = file.FileName;
                if (!file.ContentType.Contains("image"))
                    return new UnsupportedMediaTypeResult();
                var extension = Path.GetExtension(fileName);
                if (extension.Contains("."))
                    extension = extension.Replace(".", "");
                var newName = "avatar_"+CharUtil.GetRandomName(4, 6) + u.Id + "." + extension;
                var rootPath = _env.WebRootPath + "/resources";
                var filePath = rootPath + "/user/" + newName;
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
                if (!Directory.Exists(rootPath + "/user"))
                    Directory.CreateDirectory(rootPath + "/user");
                using (var fs = file.OpenReadStream())
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    long bufferSize = 124000;
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
                u.AvatarImg = "user/" + newName;

                _user.Update((u.Id), u);

                return Ok(u.AvatarImg);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadBackground(IFormFile file)
        {
            string userid = HttpContext.User.FindFirst("id").Value;
            User u = await _user.GetById((userid));
            if (u == null) return Unauthorized();
            if (file != null)
            {

                if (u.BackgroundImg != null && u.BackgroundImg.Length > 0)
                {
                    var curPath = _env.WebRootPath + "/resources/" + u.BackgroundImg;
                    if (System.IO.File.Exists(curPath))
                    {
                        System.IO.File.Delete(curPath);
                    };
                }

                var fileName = file.FileName;
                if (!file.ContentType.Contains("image"))
                    return new UnsupportedMediaTypeResult();
                var extension = Path.GetExtension(fileName);
                if (extension.Contains("."))
                    extension = extension.Replace(".", "");
                var newName = "bg_" + CharUtil.GetRandomName(4, 6) + u.Id + "." + extension;
                var rootPath = _env.WebRootPath + "/resources";
                var filePath = rootPath+"/user/" + newName;
                if (!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);
                if (!Directory.Exists(rootPath + "/user"))
                    Directory.CreateDirectory(rootPath + "/user");
                using (var fs = file.OpenReadStream())
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    long bufferSize = 124000;
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
                u.BackgroundImg = "user/" + newName;

                _user.Update((u.Id), u);

                return Ok(u.AvatarImg);
            }
            else
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Validate user from input and return the ClaimIdentity. The ClaimIdentity is used for creating a JWT Bearer token.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            User user = await ((UserDAO)_user).FindByUsername(userName);
            if (user == null) return await Task.FromResult<ClaimsIdentity>(null);
            string pwd = user.Password;
            if (!pwd.Equals(CharUtil.AesEnc(password)))
                return await Task.FromResult<ClaimsIdentity>(null);

            Permission p = await _perm.GetById((user.Permission));
            // check the credentials
            return await Task.FromResult(_jwt.GenerateId(userName, user.Id.ToString(), p.Category));
        }
        

    }

}