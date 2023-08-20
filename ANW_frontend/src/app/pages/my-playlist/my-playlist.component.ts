import { Component, OnInit, OnDestroy, ViewChild, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { PlaylistService } from '../../services/playlist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Playlist } from '../../models/playlist';
import { LanguageService } from '../../services/language.service';
import { DeletePlaylistComponent } from './delete-playlist/delete-playlist.component';
import { MusicPlayerService } from '../../services/music-player.service';
import { LoaderService } from '../../services/loader.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { observable } from 'mobx-angular';
import { SimpleTransition } from '../../components/transitions/simple.transition';
import { UserService } from 'src/app/services/user.service';

@Component({
  changeDetection: ChangeDetectionStrategy.Default,
  selector: 'app-my-playlist',
  templateUrl: './my-playlist.component.html',
  styleUrls: ['./my-playlist.component.css'],
  animations:[ SimpleTransition ]
})
export class MyPlaylistComponent implements OnInit, OnDestroy {

  @observable maxPage:number=1;
  @observable pages: number[];
  @observable page: number=1;
  @observable playlists: Playlist[];
  songlists={};
  isLoading=false;
  private paramSubcr: Subscription;

  @ViewChild(DeletePlaylistComponent) modal: DeletePlaylistComponent;

  baseUrl="/management/myplaylist";
  constructor(private playlistService: PlaylistService,
    private actRoute: ActivatedRoute,
    private router: Router,
    public lang: LanguageService,
    public player: MusicPlayerService,
    private loader: LoaderService,
    private detector: ChangeDetectorRef,
    private userService: UserService
  ) { }

  ngOnInit() {
    window.scrollTo(0,0);
    this.paramSubcr=this.actRoute.queryParams.subscribe(val=>
    {
      if (val['page'])
        this.page=val['page'];
      else{
        this.router.navigate([this.baseUrl], {queryParams: {page: 1}} );
        return;
      }
      this.getPage(this.page);
    })
  }

  getPage(page: number, limit: number=8){
    this.loader.isSubLoading=true;
    this.playlistService.getpage(page, limit).subscribe(res=>{
      if (this.page!=res.currentPage)
      {
        this.router.navigate(['.'],{queryParams:{page: res.currentPage}});
        return;
      }
      this.loader.isSubLoading=false;
      this.maxPage=res.maxPage;
      this.playlists=res.list;
      this.calcPage();
    },err =>{
      if (err.status==404 && page==1){
        this.loader.isSubLoading=false;
      }
      else{
        if (err.status==404)
          this.router.navigate(['/notfound']);
        else MaterializeToastService.send(this.lang.ui.message_connectfail,"red");
      }
    })
  }

  setPage(page: number){
    if (page<=0 || page>this.maxPage)
      return;
    this.page=page;
    this.getPage(page,8);
  }

  calcPage(){
    let lowerBound=this.page-4;
    let upperBound=lowerBound<=1?8: lowerBound+7;
    this.pages=[];
    for (var i=lowerBound<1?1:lowerBound; i<this.maxPage; i++)
      if (i>=lowerBound && i<=upperBound)
        this.pages.push(i);
  }

  ngOnDestroy(){
    if (this.paramSubcr)
      this.paramSubcr.unsubscribe();
  }

  delete(playlist:Playlist){
    this.modal.open(playlist);
  }

  onDeleted(playlist: Playlist){
    const index=this.playlists.findIndex(x=>x.Id==playlist.Id);
    this.playlists.splice(index,1);
    //this.getPage(this.page);
    this.calcPage();
    this.detector.detectChanges();
  }

  get(playlist:Playlist){
    if (!this.songlists[playlist.Id]){
      this.songlists[playlist.Id]=[];
      this.playlistService.get(playlist.Id).subscribe(res=>{
        this.songlists[playlist.Id]=res.songlist;
      })
    }
  }

  play(playlist:Playlist){
    if (this.isLoading)
      return;
    if (this.songlists[playlist.Id] && this.songlists[playlist.Id].length>0)
    {
      this.player.currentPlaylist=playlist;
      this.player.playlist=this.songlists[playlist.Id];
      this.player.playSong(this.songlists[playlist.Id][0]);
    }
    else{
      this.isLoading=true;
      this.playlistService.get(playlist.Id).subscribe(res=>{
        this.isLoading=false;
        this.songlists[playlist.Id]=res.songlist;
        this.player.currentPlaylist=res.playlist;
        this.player.playlist=res.songlist;
        if (res.songlist && res.songlist.length>0)
          this.player.playSong(res.songlist[0]);
      },err=>{
        this.isLoading=false;
      })
    }
  }
  get isManager(){
    return this.userService.IsAuthenticated && this.userService.getUserLevel()>=9;
  }
}
