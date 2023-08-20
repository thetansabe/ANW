using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/resources")]
    public class ResourceController : Controller
    {
        private readonly IHostingEnvironment _env;
        public ResourceController(IHostingEnvironment env)
        {
            _env = env;
        }
        // GET: api/<controller>
        [HttpGet("image/{name}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string name)
        {
            #region Prevent Outsite Request
            StringValues reff = new StringValues();
            HttpContext.Request.Headers.TryGetValue("Referer", out reff);

            if (reff.Count <= 0)
                return NotFound();
            #endregion
            string basePath = _env.WebRootPath + "/resources/image/";
            //===================================================
            FileStream fs;
            GZipStream gz;
            try
            {
                fs = new FileStream(basePath + name, FileMode.Open, FileAccess.Read, FileShare.Read, 4069, FileOptions.Asynchronous);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            #region Resolve the request to partly content
            long fSize = fs.Length;
            long startbyte = 0;
            long endbyte = fSize - 1;
            long maxBuffer = 1024;
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

            Response.ContentType = "image/" + Path.GetExtension(name).Substring(1);
            Response.Headers.Add("Content-Accept", Response.ContentType);
            Response.Headers.Add("Content-Length", desSize.ToString());
            Response.Headers.Add("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
            Response.Headers.Add("Accept-Ranges", "bytes");
            Response.Headers.Remove("Cache-Control");
            #endregion
            //Data
            fs.Seek(startbyte, SeekOrigin.Begin);
            return File(fs, Response.ContentType);
        }
        
    }
}
