using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ANWebServices.Controllers
{
    [Route("api/lookup")]
    public class LookupController : Controller
    {
        private readonly IRepository<Song> _song;
        private readonly IRepository<Artist> _art;
        private readonly IRepository<Album> _alb;
        private readonly IRepository<Playlist> _playlist;

        public LookupController(
            IRepository<Song> song, IRepository<Artist> art,
            IRepository<Album> alb, IRepository<Playlist> playlist
            )
        {
            _song = song;
            _art = art;
            _alb = alb;
            _playlist = playlist;
        }

        [HttpPost]
        [AllowAnonymous]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> Lookup([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            string key = keyword.Trim().ToLower();
            var songList = await ((SongDAO)_song).Lookup(key, 80);
            var playlistList = await ((PlaylistDAO)_playlist).Lookup(key, 40);
            if (songList.Count == 0 && playlistList.Count == 0)
            {
                songList = await _song.Find(x => x.Tags.Contains(keyword), 80, 0);
                playlistList = await _playlist.Find(x => x.Tags.Contains(keyword), 40, 0);

                if (songList.Count == 0 && playlistList.Count == 0)
                {
                    return NoContent();
                }
            }
            if (page <= 0) page = 1;
            int offset = (page - 1) * size;
            if (offset < songList.Count)
                for (var i=offset; i<offset+size; i++)
                    if (i<songList.Count)
                    {
                        foreach (string aid in songList[i].Artists)
                            songList[i].ArtistList.Add(await _art.GetById(aid));
                    }

            int maxPage = (int) Math.Ceiling(songList.Count / (double) size);
            

            int maxSizeSong = songList.Count - offset < size ? songList.Count - offset : size;
            int maxSizePlaylist = playlistList.Count - offset < size ? playlistList.Count - offset : size;

            var json = JsonConvert.SerializeObject(new
            {
                songList = songList.GetRange(offset, maxSizeSong),
                playlist= playlistList.Count>0?playlistList.GetRange(0,maxSizePlaylist):null,
                maxPage
            });
            return Ok(json);
        }
        [HttpPost("more")]
        [AllowAnonymous]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> LookupMore([FromQuery] string keyword)
        {
            string key = keyword.Trim().ToLower();
            var artistList = await ((ArtistDAO)_art).Lookup(key, 20);
            var albumList = await ((AlbumDAO)_alb).Lookup(key, 20);
            if (artistList.Count == 0 && albumList.Count == 0)
            {
                artistList = await (_art).Find(x => x.Name.Contains(keyword), 15, 0);
                albumList = await _alb.Find(x=>x.Tags.Contains(keyword), 15,0);

                if (artistList.Count == 0 && albumList.Count == 0)
                {
                    return NoContent();
                }
            }

            var json = JsonConvert.SerializeObject(new
            {
                albumList,
                artistList
            });
            return Ok(json);
        }
    }
}
