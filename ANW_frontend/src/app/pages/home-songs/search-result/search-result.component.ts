import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { LanguageService } from 'src/app/services/language.service';
import { LookupService } from 'src/app/services/lookup.service';
import { observable } from 'mobx-angular';
import { Song } from 'src/app/models/song';
import { Artist } from 'src/app/models/artist';
import { Album } from 'src/app/models/album';
import { Playlist } from 'src/app/models/playlist';
import { MusicPlayerService } from 'src/app/services/music-player.service';
import { ButtonTransition } from 'src/app/components/transitions/button.transition';
import { environment } from '../../../../environments/environment';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-search-result',
  templateUrl: './search-result.component.html',
  styleUrls: ['./search-result.component.css'],
  animations: [ButtonTransition]
})
export class SearchResultComponent implements OnInit {

  @observable isLoading: boolean = false;
  @observable songList: Song[];
  @observable artistList: Artist[];
  @observable albumList: Album[];
  @observable playlist: Playlist[];
  @observable responseMs: number;
  @observable maxPage: number;
  @observable currentPage: number;

  songEmpty: boolean=false;
  artistEmpty: boolean=false;

  constructor(
    private router: Router,
    private actRoute: ActivatedRoute,
    private lookupService: LookupService,
    public player: MusicPlayerService,
    public lang: LanguageService
  ) { }

  ngOnInit() {
    this.actRoute.queryParams.subscribe(params=>{
      if (params['keyword']){
        let page= +params['page'] || 1;
        if (page<=0) page=1;
        let size= +params['size'] || 6;
        if (size<=0) {
          this.router.navigate(['/notfound']);
          return;
        }
        this.lookup(params['keyword'],page,size);
      }
      else this.router.navigate(['/notfound']);
    });
    window.scrollTo(0,0);
  }


  lookup(keyword: string, page: number=1, size: number=6){
    this.isLoading=true;
    const now=new Date();
    this.lookupService.findMusic(keyword,page,size).subscribe(result=>{
      this.isLoading=false;
      this.maxPage=result.maxPage;
      const future= new Date();
      this.responseMs= (future.getTime()- now.getTime())/1000;
      this.songList=result.songList;
      this.playlist=result.playlist;
      if (this.songList.length==0
        && this.playlist.length==0
        )
        {
          this.songEmpty=true;
          if (this.artistEmpty)
            this.router.navigate(['/notfound']);
          return;
        }
    },err =>{
      this.isLoading=false;
      const future= new Date();
      this.responseMs= (future.getTime()- now.getTime())/1000;
      if (err.status==404 && this.artistEmpty)
      {
        this.router.navigate(['/notfound']);
      }
    })
    this.lookupService.findArtist(keyword).subscribe(result=>{
      this.albumList=result.albumList;
      this.artistList=result.artistList;
      if (this.albumList.length==0
        && this.artistList.length==0
        )
        {
          this.artistEmpty=true;
          if (this.songEmpty)
            this.router.navigate(['/notfound']);
          return;
        }
    },err =>{
      if (err.status==404 && this.songEmpty)
      {
        this.router.navigate(['/notfound']);
      }
    })
  }

  resolveResource(target: string, isResource=false){
    return target ? (isResource?environment.RSRC_URL:environment.MEDIA_URL)+target:'';
  }
}
