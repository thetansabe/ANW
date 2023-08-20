import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, AfterViewInit } from '@angular/core';
import { Song } from '../../../models/song';
import { SongService } from '../../../services/song.service';
import { ActivatedRoute, Router } from '../../../../../node_modules/@angular/router';
import { Subscription } from '../../../../../node_modules/rxjs';
import { LanguageService } from '../../../services/language.service';
import { MusicPlayerService } from '../../../services/music-player.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { observable, computed } from 'mobx-angular';
import { UserService } from '../../../services/user.service';
import { User } from '../../../models/user';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { FacebookService, InitParams } from 'ngx-facebook';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-song-detail',
  templateUrl: './song-detail.component.html',
  styleUrls: ['./song-detail.component.css'],
  animations:[
    trigger("button",[
      transition(":enter",[
        style({ opacity: 0, transform:'rotate(90deg)'}),
        animate("0.3s 50ms ease",style({ opacity: 1, transform: 'rotate(0)'}))
      ]),
      transition(":leave",[
        style({ opacity: 1, transform:'rotate(0)'}),
        animate("0.2s linear",style({ opacity: 0, transform: 'rotate(-90deg)'}))
      ])
    ])
  ]
})
export class SongDetailComponent implements OnInit,OnDestroy, AfterViewInit {

  @observable currentSong: Song;
  @observable duration: string;

  @observable profile: User;

  private subcriber:Subscription;
  constructor(
    private user: UserService,
    private songService: SongService,
    private actRoute: ActivatedRoute,
    private router: Router,
    private fbService: FacebookService,
    public lang: LanguageService,
    public player: MusicPlayerService
  ) { }

  ngOnInit() {
    window.scrollTo(0,0);
    this.subcriber=this.actRoute.params.subscribe(params=>{
      if (params['id'])
      {
        this.get(params['id']);
      }
      else this.router.navigate(['/notfound']);
      console.log(params);
    })
    this.user.profile.subscribe(profile=>this.profile=profile);
  }
  ngAfterViewInit(){
    this.initFbComment();
  }

  get(id: string){
    this.songService.get(id).subscribe(val=>{
      this.currentSong=val;
      if (val.Duration){
        var duraSecond=+val.Duration-1;
        const mins=Math.floor(duraSecond/60);
        const seconds=duraSecond%60;
        this.duration=mins+":"+(seconds<10?'0'+seconds:seconds);
      }
    },err=>{
      this.router.navigate(['/notfound']);
    })
  }

  ngOnDestroy(){
    this.subcriber.unsubscribe();
  }

  get isLoggedIn(){
    return this.user.IsAuthenticated;
  }
  
  get currentUrl(){
    return window.location.href;
  }

  favorite(song:Song){
    if (this.user.IsAuthenticated){
      this.songService.toggleFavorite(song).subscribe(val=>{
        this.profile.SongFavorite=val;
        this.user.next(this.profile);
      },err=>{
        let msg=this.lang.ui.message_requestfail;
        if (err.status==0)
        {
          msg=this.lang.ui.message_connectfail;
        }
        MaterializeToastService.send(msg,"rounded red");
      })
    }
  }

  isFavorited(song: Song){
    return this.profile && this.profile.SongFavorite.includes(song.Id);
  }
  
  initFbComment(){
    let params:InitParams={
      appId:'262137977830257',
      xfbml: true,
      version:'v3.2'
    }

    this.fbService.init(params);
    
    // var js, fjs = document.getElementsByTagName("script")[0];
    // if (document.getElementById(id)) return;
    // js = document.createElement("script"); js.id = id;
    // js.src = 'https://connect.facebook.net/vi_VN/sdk.js#xfbml=1&version=v3.2&appId=262137977830257&autoLogAppEvents=1';
    // fjs.parentNode.insertBefore(js, fjs);
  }
}
