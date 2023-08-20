import { Component, OnInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { LanguageService } from '../../services/language.service';
import { MusicPlayerService } from '../../services/music-player.service';
import { Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { PlaylistService } from '../../services/playlist.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { UserService } from '../../services/user.service';
import { OpenPlaylistComponent } from './open-playlist/open-playlist.component';
import { ResetPlaylistComponent } from './reset-playlist/reset-playlist.component';
import { trigger, transition, style, animate } from '@angular/animations';
import {  observable } from 'mobx-angular';
import { Song } from '../../models/song';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'music-player',
  templateUrl: './music-player.component.html',
  styleUrls: ['./music-player.component.css'],
  animations:[
    trigger("playlistItem",[
      transition(":enter",[
        style({ transform: "translateX(30px)", opacity:0}),
        animate("200ms ease-in-out",style({ transform:"translateX(0)", opacity: 1}))
      ]),
      transition(":leave",[
        style({ transform: "translateX(0)", opacity:1}),
        animate("150ms linear",style({ transform:"translateX(40px)", opacity: 0}))
      ])
    ]),
    trigger("fabbutton",[
      transition(":enter",[
        style({ opacity: 0, transform:'rotate(90deg)'}),
        animate("0.3s 50ms ease", style({ opacity: 1, transform: 'rotate(0)'}))
      ]),
      transition(":leave",[
        style({ opacity: 1, transform:'rotate(0)', position:'absolute', left:'0'}),
        animate("0.2s ease", style({ opacity: 0, transform: 'rotate(-90deg)'}))
      ])
    ])
  ]
})
export class MusicPlayerComponent implements OnInit {
  @observable player_height: number= 360;
  player_background:string='white';
  @observable opacity_range: number= 9;
  @observable playerToggle=false;
  @observable volume:number=100;
  @observable repeatMode: number =0;
  currentSong: Song;
  timeEst={
    gotoDuration: '',
    isHovering: false,
    top: 0,
    listener:new Subject<any>()
  }
  @observable isRequesting=false;
  

  @ViewChild('timetip') timeTip:any;
  @ViewChild(OpenPlaylistComponent) modal: OpenPlaylistComponent;
  @ViewChild(ResetPlaylistComponent) reset: ResetPlaylistComponent;
  constructor(public lang: LanguageService, public player: MusicPlayerService,
    private playlist: PlaylistService, private user: UserService
  ) { }

  ngOnInit() {
    this.onHoverListener();
    this.volume=this.player.volume*100;
    this.player.currentSong.subscribe(val=>this.currentSong=val);
  }

  showPlayer(){
    this.playerToggle=true;
  }
  hidePlayer(){
    this.playerToggle=false;
  }

  togglePlaying(){
    if (this.player.isPlaying)
      this.player.pause();
    else this.player.play();
  }
  onVolumeChanged(){
    this.player.setVolume(this.volume);
  }

  seek(ev){
    if (ev.isTrusted && this.player.currentSong.getValue()){
      const screenWidth=document.documentElement.clientWidth;
      const clickX=ev.clientX;
      const percent=clickX/screenWidth;
      this.player.setCurrentTime(this.player.duration*percent);
    }
  }

  onEnter(ev){
    if (ev.isTrusted){
    this.timeEst.top=ev.srcElement.getBoundingClientRect().top;}
  }
  onHover(ev){
    this.timeEst.listener.next(ev);
  }
  onHoverListener(){
    this.timeEst.listener
    .pipe(
      map(res=> res)
    )
    .subscribe(ev=>{
      if (!ev) {
        this.timeEst.isHovering=false;
        return;
      }
      if (ev.isTrusted && this.player.currentSong.getValue()){
        this.timeEst.isHovering=true;
        const screenWidth=document.documentElement.clientWidth;
        const clickX=ev.clientX;
        const percent=clickX/screenWidth;
        const estTime=Math.round(this.player.duration*percent);
        const min=Math.floor(estTime/60);
        const sec=estTime%60;
        this.timeEst.gotoDuration=min+":";
        if (sec<10) this.timeEst.gotoDuration+='0'+sec;
        else this.timeEst.gotoDuration+=sec;
        const itemWidth=this.timeTip.nativeElement.offsetWidth;
        const itemHeight=this.timeTip.nativeElement.offsetHeight;
        const newY=this.timeEst.top-itemHeight-10;
        let newX=clickX-itemWidth/2;
        if (newX<10) newX=10;
        else if ((newX+itemWidth+20)>screenWidth)
          newX=screenWidth-20 - itemWidth/2;
        this.timeTip.nativeElement.style.left=newX+'px';
        this.timeTip.nativeElement.style.top=newY+'px';
      }
    });
  }
  onLeave(ev){
    this.timeEst.listener.next(null);
  }
  save(){
    this.isRequesting=true;
    this.player.currentPlaylist.Collection=[];
    for (var i=0; i<this.player.playlist.length;i++)
      this.player.currentPlaylist.Collection.push(this.player.playlist[i].Id);
    this.playlist.save(this.player.currentPlaylist).subscribe(val=>{
      this.player.currentPlaylist=val;
      this.player.isAltered=false;
      this.isRequesting=false;
    },err=>{
      this.isRequesting=false;
      let msg=this.lang.ui.message_requestfail;
      if (err.status=401)
      {
        msg=this.lang.ui.message_unauthorized;
      }
      MaterializeToastService.send(msg, "rounded red");
    })

  }

  openReset(){
    this.reset.open();
  }

  get isLoggedIn(){
    return this.user.IsAuthenticated;
  }

  get userId(){
    return this.user.profile.getValue().Id;
  }


  onRepeatModeChanged(){
    this.player.repeat=this.repeatMode;
  }
}
