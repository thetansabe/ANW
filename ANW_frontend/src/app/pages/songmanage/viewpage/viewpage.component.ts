import { Component, OnInit, ViewChild, ElementRef, ChangeDetectionStrategy } from '@angular/core';
import { ActivatedRoute, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Song } from '../../../models/song';
import { SongManagerService } from '../../../services/song-manager.service';
import { LanguageService } from '../../../services/language.service';
import { MusicPlayerService } from '../../../services/music-player.service';
import { trigger, transition, stagger,query, animate, style} from '@angular/animations';
import { LoaderService } from '../../../services/loader.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { UserService } from '../../../services/user.service';
import { FormGroup, FormBuilder } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { computed, observable } from 'mobx-angular';
import { EditSongComponent } from '../edit-song/edit-song.component';

@Component({
  changeDetection: ChangeDetectionStrategy.Default,
  selector: 'app-viewpage',
  templateUrl: './viewpage.component.html',
  styleUrls: ['./viewpage.component.css'],
  animations:[
    trigger('transition',[
      // Begin trigger
      transition("*=>*",[
        query("tr:enter",style({ opacity: 0 , display: 'none', transform:'scaleX(0.1)'}), {optional:true}),
        query("tr:enter", stagger(50, [
          style({display: 'none'}),
          animate(200, style({ opacity:1, transform:'scaleX(1)'}))
        ]), {optional:true})
      ])
      // End trigger
    ])
  ]
})
export class ViewpageComponent implements OnInit {
   currentPage: number=1;
   pages:number[]=[];
   songList: Song[]=[];
   selectedSongList: Song[]=[];
   songchecker: boolean[]=[];
   filterType: string="0";
   selectedSong: Song;
   artistList: any={};
  filterForm: FormGroup;
  
  isLoading: boolean=false;

  baseUrl='/management/song-manager';

  maxSize: number=25;
  @ViewChild('songTable') songTable: ElementRef;
  @ViewChild(EditSongComponent) editModal: EditSongComponent;
  constructor(private actRoute: ActivatedRoute, 
    public loader: LoaderService, 
    private router: Router, 
    public player: MusicPlayerService, 
    private songService: SongManagerService, 
    public lang: LanguageService,
    private user: UserService,
    private builder: FormBuilder
  ) { }

  ngOnInit() {
    this.filterForm=this.builder.group({
      filterInput:''
    });
    this.initFilterForm();
    this.actRoute.params.subscribe(result=>{
      this.currentPage=+result['pageNumber'];
      this.fetch();
    });

    const routeData=this.actRoute.snapshot.data;
    if (routeData && routeData['defaultRoute'])
      this.baseUrl=routeData['defaultRoute'];
  }

  fetch(){
    this.onFilterChanged();
  }

  countCheckedSongs(){
    return this.songchecker.filter(x=>x==true).length;
  }

  initFilterForm(){
    this.filterForm.controls['filterInput'].valueChanges
    .pipe( debounceTime(750) )
    .subscribe(val=>{
      this.onFilterChanged();
    })
  }

  toggleChecker(){
    const isChecked=this.isCheckedAll;
    for (var i=0; i< this.songList.length;i++)
      this.songchecker[i]=!isChecked;
  }

  onFilterChanged(){
    this.loader.isSubLoading=true;
    let toggle=+this.filterType-1;
    let keywords='';
    if (this.filterForm.get('filterInput').value && this.filterForm.get('filterInput').value.length)
      keywords=this.filterForm.get('filterInput').value;
    this.songService.get(this.currentPage,this.maxSize, toggle, keywords).subscribe(val=>{
      if (val.pageResult && val.pageResult!=this.currentPage)
      {
        this.router.navigate([this.baseUrl,val.pageResult]);
        return;
      }
      this.scrollTop();

      this.loader.isSubLoading=false;
      this.pages=[];
      this.songchecker=[];
      this.songList=val.list;
      val.artistList.forEach(artist=>{
        this.artistList[artist.Id]=artist;
      })
      let lowBound=this.currentPage-7;
      let upBound=this.currentPage+7;
      for (var i=lowBound; i< upBound;i++)
        if (i<0) i=-1;
        else if (i>=val.totalpage) break;
        else
          this.pages.push((i+1));
    },err=>{
      this.loader.isSubLoading=false;
      if (err.status==404){
        this.router.navigate([this.baseUrl,1]);
      }
    })
  }
  setCurrentSong(song:Song){
    this.selectedSong=song;
    if (this.selectedSongList)
      this.selectedSongList=[];
  }
  setListOfSong(){
    this.selectedSong=null;
    this.selectedSongList=[];
    for (var i=0; i< this.songchecker.length;i++)
      if (this.songchecker[i])
        this.selectedSongList.push(this.songList[i]);
  }

  approve(allow: number){
    let list: string[]=[];
    for (var i=0; i< this.songchecker.length;i++)
      if (this.songchecker[i])
        list.push(this.songList[i].Id);
    //this.loader.isSubLoading=true;
    this.user.decode();
    this.isLoading=true;
    this.songService.approve(list,allow).subscribe(res=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestsuccess);
      this.songchecker=[];
      for (var i=0; i<list.length; i++)
      {
        this.songList.find(x=>x.Id==list[i]).Approved=allow;
      }
      //this.fetch();
    },err=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,'rounded red');
    })
  }

  scrollTop(){
    const elem:HTMLElement=this.songTable.nativeElement as HTMLElement;
    if (window.scrollY+150>elem.offsetTop)
      window.scrollTo(0,elem.offsetTop-50);
  }
  
  edit(song: Song){
    this.editModal.open(song, this.artistList);
  }

  onSaved(e: Song){
    const song= this.songList.find(x=>x.Id==e.Id);
    song.SubType=e.SubType;
    song.SelfPerformance=e.SelfPerformance;
    song.ArtistList=e.ArtistList;
    song.Artists=e.Artists;
    song.Name=e.Name;
  }

  @computed get isAdmin(){
    return this.user.IsAuthenticated && this.user.getUserLevel()>=9;
  }

  @computed get isCheckedAll(){
    return this.songchecker && this.songList && this.songList.length>0 &&
      this.songchecker.length==this.songList.length && this.songchecker.every(x=>x);
  }
}
