import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { SongService } from '../../services/song.service';
import { UserService } from '../../services/user.service';
import { MusicPlayerService } from '../../services/music-player.service';
import { Song } from '../../models/song';
import { Router, ActivatedRoute } from '../../../../node_modules/@angular/router';
import { User } from '../../models/user';
import { LanguageService } from '../../services/language.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { LoaderService } from '../../services/loader.service';
import { SimpleTransition } from '../../components/transitions/simple.transition';

@Component({
  changeDetection: ChangeDetectionStrategy.Default,
  selector: 'app-my-favorite',
  templateUrl: './my-favorite.component.html',
  styleUrls: ['./my-favorite.component.css'],
  animations: [ SimpleTransition ]
})
export class MyFavoriteComponent implements OnInit {

  songList: Song[];
  list: Song[];
  isRequesting={};
  page: number;
  pages: number[];
  maxPage: number;

  limit:number=9;
  favorList: string[];

  baseUrl="/management/favorite";
  constructor(
    private loader: LoaderService,
    private songService: SongService,
    private userService: UserService,
    public player: MusicPlayerService,
    private router: Router,
    public lang: LanguageService,
    private actRoute: ActivatedRoute
  ) { }

  ngOnInit() {
    window.scrollTo(0,0);
    this.loader.isSubLoading=true;
    this.songService.getFavorite().subscribe(res=>{
      this.loader.isSubLoading=false;
      this.songList=res;
      if (!res) return;
      this.maxPage=Math.ceil(res.length/this.limit);
      this.pages=[];
      for (var i=1;  i<=this.maxPage; i++)
        this.pages.push(i);
      if (this.page)
        this.changeEntry();
    }, err=>{
      if (err.status==401)
        this.router.navigate(["/login"],{queryParams:{ returnRoute:this.baseUrl}});
      else this.router.navigate(["/"]);
    });
    this.actRoute.queryParams.subscribe(params=>{
      if (params['page'] && !isNaN(params['page']))
        this.page=+params['page'];
      else this.page=1;
      if (this.songList)
        this.changeEntry();
    })
    this.userService.myProfile.subscribe(p=> {
      this.favorList=p.SongFavorite;
      console.log(p);
    });
  }

  setPage(page: number){
    if (page<=0 || page>this.maxPage) return;
    this.page=page;
    this.router.navigate([this.baseUrl],{queryParams:{ 'page':page}});
  }

  changeEntry(){
    if (this.songList && this.page)
    {
      if (this.page<=0 || this.page>this.maxPage){
        this.router.navigate(['/notfound']);
        return;
      }
      const offset=(this.page-1)*this.limit;
      let end=offset+this.limit;
      if (end>this.songList.length)
        end=this.songList.length;
      this.list=this.songList.slice(offset,end);
    }
  }

  playAll(){
    this.player.currentPlaylist=null;
    const favorite=this.favorList;
    const songs=this.songList.filter(x=>favorite.includes(x.Id));
    const contain=favorite.every(x=> this.player.playlist.some(s=>s.Id==x));
    //console.log(contain);
    if (!contain){
      this.player.playlist=songs;
      this.player.playSong(this.player.playlist[0]);
    }
  }

  favorite(song: Song){
    if (this.userService.IsAuthenticated){
      if (this.isRequesting[song.Id]) return;
      this.isRequesting[song.Id]=true;
      this.songService.toggleFavorite(song).subscribe(val=>{
        this.isRequesting[song.Id]=false;
        this.favorList=val;
        const profile: User=this.userService.profile.getValue();
        profile.SongFavorite=val;
        this.userService.next(profile);
      },err=>{
        this.isRequesting[song.Id]=false;
        let msg=this.lang.ui.message_requestfail;
        if (err.status==0)
        {
          msg=this.lang.ui.message_connectfail;
        }
        MaterializeToastService.send(msg,"rounded red");
      })
    }
  }

  isFavorite(song: Song){
    //console.log(this.favorList);
    return this.favorList && this.favorList.includes(song.Id);
  }
}
