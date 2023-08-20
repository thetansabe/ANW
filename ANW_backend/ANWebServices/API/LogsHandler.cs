using Amnhac.Interfaces;
using Amnhac.Models;
using Amnhac.Repositories;
using ANWebServices.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ANWebServices.API
{
    public class LogsHandler: IHostedService, IDisposable
    {
        private const double THREAD_SLEEP=180;
        private const int CLEAN_PERIOD = 120;
        private Thread runner;
        private IRepository<Song> _song;
        private IRepository<SongLogs> _logger;
        private IRepository<SongRanked> _ranker;
        private IRepository<SongType> _type;
        public LogsHandler(IOptions<DbSettings> settings, IRepository<SongLogs> logger, 
            IRepository<SongRanked> ranker)
        {
            _song = new SongDAO(settings);
            _logger = logger;
            _ranker = ranker;
            _type = new TypeDAO(settings);
        }

        public void handle()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                List<SongRanked> listFilter=null;
                bool Runnable = true;
                try
                {
                    listFilter = ((SongRankDAO)_ranker).GetAllInRange(now).Result;
                    Console.WriteLine(JsonConvert.SerializeObject(listFilter));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Runnable = false;   
                }
                if (Runnable && listFilter!=null)
                {
                    List<Song> songs = new List<Song>();
                    for (int i = 0; i < listFilter.Count; i++)
                        if (listFilter[i].LastUpdate.Date != now.Date && listFilter[i].LastUpdate < now)
                            songs.Add(_song.GetById((listFilter[i].SongId)).Result);
                    Console.WriteLine("Currently handling logging: " + songs.Count + " total song(s)");
                    for (int i = 0; i < songs.Count; i++)
                    {
                        string songId = songs[i].Id.ToString();
                        ((SongLogsDAO)_logger).Clear(now.Subtract(TimeSpan.FromDays(CLEAN_PERIOD)));
                        SongLogs log = _logger.GetById((songs[i].Id)).Result;
                        SongRanked type = _ranker.Find(x => x.SongId==songId).Result.FirstOrDefault();
                        string category = "";
                        if (type != null) category = type.Category;
                        if (log != null && log.CreatedOn.Date < now.Date)
                        {
                            if (log.ViewCount != songs[i].View)
                            {
                                SongLogs newlog = new SongLogs()
                                {
                                    Id= ObjectId.GenerateNewId().ToString(),
                                    SongId = songId,
                                    SongCategory= category,
                                    ViewCount = songs[i].View,
                                    CreatedOn = now
                                };
                                _logger.Insert(newlog);
                            }
                        }
                        else if (log == null)
                        {
                            SongLogs newlog = new SongLogs()
                            {
                                Id = ObjectId.GenerateNewId().ToString(),
                                SongId = songId,
                                SongCategory = category,
                                ViewCount = songs[i].View,
                                CreatedOn = now
                            };
                            _logger.Insert(newlog);
                        }
                        _ranker.Update((songs[i].Id),_ranker.GetById(songs[i].Id).Result);
                    }
                }
                try
                {
                    Thread.Sleep(TimeSpan.FromMinutes(THREAD_SLEEP));
                }
                catch (ThreadInterruptedException)
                {
                    return;
                }
                GC.Collect();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            runner = new Thread(this.handle);

            runner.Start();
            Console.WriteLine("LogHandler has started.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("LogHandler has been stopped.");
            runner.Interrupt();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
