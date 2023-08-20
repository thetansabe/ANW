import { Injectable } from '@angular/core';
import { Subject, BehaviorSubject } from 'rxjs';
import { Song } from '../models/song';
import { environment } from '../../environments/environment';
import { LanguageService } from './language.service';
import { Playlist } from '../models/playlist';
import { MaterializeToastService } from '../directives/MaterializeR/materialize-toast.service';
import { computed, observable } from 'mobx-angular';

declare var M: any;
@Injectable({
  providedIn: 'root'
})
export class MusicPlayerService {
  private readonly URL=environment.HOST+"/api/song/play/";
  private player: HTMLAudioElement;
  private src: string;
  private playable:boolean=false;
  buffered: number;
  byteloaded: number;
  @observable isPlaying: boolean=false;
  @observable isLoading:boolean=false;
  @observable isAltered:boolean=false;
  @observable currentTime: number;
  @observable duration: number;
  @observable currentSong: BehaviorSubject<Song>=new BehaviorSubject<Song>(null);
  @observable playlist: Song[];
  @observable currentPlaylist: Playlist;
  @observable controlDisabled:boolean=false;
  repeat:number=0;
  repeatTime: number=0;
  constructor(private lang: LanguageService) { 
    this.player=new Audio();
    this.playlist=[];
    let self=this;
    this.player.addEventListener('loadeddata', function(){
      if (self.player.src)
        {
          self.playable=true;
          self.duration=self.player.duration;
          self.play();
          self.src=self.player.src;
          self.isLoading=false;
        }
    });
    this.player.addEventListener('error', ((e)=>this.catchError(e,this)).bind(this));
    this.player.addEventListener('playing',function(){
      if (self.playable){
        self.byteloaded =self.player.buffered.end(0);
        self.duration =self.player.duration;
      }
      self.isPlaying=true;
    });
    this.player.addEventListener('timeupdate',function(){
      if (self.playable){
        self.buffered=self.player.buffered.end(0);
        self.byteloaded=self.player.buffered.end(0);
        self.currentTime=self.player.currentTime;
      }
      if (self.player.currentTime>=self.player.duration-0.1){
        self.player.currentTime=0;
        let idx=self.currentSongIndex;
        if (self.repeat==0 && (!self.currentPlaylist || !self.currentPlaylist.Id)) {
          if (self.playlist.length>1){
            if (idx<self.playlist.length-1)
              self.setSong(idx+1);
          }
          else 
            self.pause();
          self.playlist.splice(idx,1);
        }
        else{
          if (idx<self.playlist.length-1)
            self.setSong(idx+1);
          else
            if (self.repeat>0){
              if (self.repeat==1){
                if (self.repeatTime>=1)
                {
                  self.repeatTime=self.repeatTime-1;
                  if (self.repeatTime==0){
                    self.pause();
                    return;
                  }
                }
                self.setSong(0);
              }
              if (self.repeat==2 && self.repeatTime>0){
                self.setSong(0);
              }
            }
            else self.pause();
        }
      }
    });
    try{
      this.player.volume=(+localStorage.getItem("player.volume") || 100)/100;
    }
    catch (e)
    {
      this.player.volume=1;
    }
    this.player.addEventListener('pause',function(){
      self.isPlaying=false;
    });
    setInterval(()=>{
      if (self.repeat==2 && self.repeatTime>0)
        self.repeatTime--;
    },60000);
  }
  play(){
    if (this.controlDisabled) return;
    if (!this.playable || !this.player.paused) return;
    if (this.currentSong.getValue() && this.findIndex(this.currentSong.getValue())<0)
     this.addToPlaylist(this.currentSong.getValue(),true);
    this.player.play();
    this.isPlaying=true;
    this.currentTime=this.player.currentTime;
  }
  get isMute(){
    return this.player.muted;
  }
  toggleMute(){
    this.player.muted=!this.player.muted;
  }
  pause(){
    this.player.pause();
    this.isPlaying=false;
    this.currentTime=this.player.currentTime;
  }
  setVolume(percent){
    this.player.volume=percent/100;
    localStorage.setItem("player.volume",this.player.volume*100+'');
  }
  get volume(){
    return this.player.volume;
  }
  getCurrentTime(){
    return this.player.currentTime;
  }
  setCurrentTime(duration){
    this.player.currentTime=duration;
    this.currentTime=duration;
  }
  getBufferedPercentage(){
    if (this.playable)
      return (this.buffered/this.byteloaded)*100;
    return 0;
  }
  getPlayProgressPercentage(){
    if (this.playable)
      return (this.currentTime/this.duration)*100;
    return 0;
  }
  @computed get currentSongIndex(){
    return this.playlist.findIndex(val=>val.Id==this.currentSong.getValue().Id);
  }
  setSong(index){
    if (index>=0 && index<this.playlist.length)
    {
      this.currentSong.next(this.playlist[index]);
      this.setSrc(this.URL+this.currentSong.getValue().Id);
    }
  }
  findIndex(song:Song){
    return this.playlist.findIndex(val=>val && val.Id && val.Id==song.Id);
  }
  playSong(song: Song){
    var idx=this.playlist.findIndex(x=>x && x.Id && x.Id==song.Id);
    if (idx>=0)
    {
      this.setSong(idx);
    }
    else{
      let curr=this.currentSong.getValue();
      if (curr && curr.Id!=song.Id)
      {
        let currIdx=this.currentSongIndex;
        this.playlist.splice(currIdx,1,song);
      }
      else
        this.playlist.push(song);
      this.setSong(this.playlist.length-1);
    }
  }

  toggle(song: Song){
    if (this.isThatSong(song))
    {
      if (this.isPlaying)
        this.pause();
      else this.play();
      return;
    }
    else this.playSong(song);
  }

  addToPlaylist(song: Song, silent:boolean=false){
    var idx=this.playlist.findIndex(x=>x.Id==song.Id);
    if (idx>=0)
    {
      this.setSong(idx);
    }
    else{
      if (!silent) M.toast({html: song.Name+" "+this.lang.ui.addedtoplaylist, classes:'rounded'});
      this.playlist.push(song);
      if (this.currentPlaylist)
        this.isAltered=true;
      if (this.playlist.length==1 && !this.isPlaying)
        this.setSong(0);
    }
  }
  moveSong(source:number, target:number=0){
    if (source>=this.playlist.length || source<0) return;
    if (target>=this.playlist.length) target=this.playlist.length-1;
    if (target<0) target=0;
    let temp=this.playlist.splice(source,1);
    this.playlist.splice(target,0, temp[0]);
    if (this.currentPlaylist)
      this.isAltered=true;
  }
  removeSong(song: Song){
    var idx=this.playlist.findIndex(x=>x.Id==song.Id);
    if (idx>=0){
      if (this.currentSong.getValue() && song.Id== this.currentSong.getValue().Id)
      {
        if (idx<this.playlist.length-1){
          console.log(idx);
          this.playSong(this.playlist[idx+1]);
        }
        else this.pause();
      }
      this.playlist.splice(idx,1);
      if (this.currentPlaylist)
        this.isAltered=true;
    }
  }
  isThatSong(song: Song){
    return this.currentSong.getValue() && this.currentSong.getValue().Id==song.Id;
  }
  setSrc(path){
    if (path==this.src)
     {
       this.play();
       return;
      }
    this.playable=false;
    this.byteloaded=0;
    this.buffered=0;
    this.currentTime=0;
    this.duration=0;
    this.isPlaying=false;
    this.isLoading=true;
    this.player.src=path;
    this.retries=0;
  }

  private retries: number=0;
  catchError(err, player){
    if (err)
    switch (err.target.error.code){
      case err.target.error.MEDIA_ERR_ABORTED:
        player.playSong(null);
        player.pause();
        break;
      case err.target.error.MEDIA_ERR_NETWORK:
        if (player.retries<=5){
          setTimeout((()=>player.player.load()).bind(player),5000);
          player.retries++;
        }
        else
          MaterializeToastService.send(player.lang.ui.message_connectfail,"red");
        break;
    }
  }
}
