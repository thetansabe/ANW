import { Component, OnInit, AfterViewInit, ViewChild, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { SongService } from '../../services/song.service';
import { Song } from '../../models/song';
import { MusicPlayerService } from '../../services/music-player.service';
import { ChartDataset, ChartJsDirective } from '../../directives/ChartJS/chart-js.directive';
import { LanguageService } from '../../services/language.service';
import { LoaderService } from '../../services/loader.service';
import { MaterializeTabsDirective } from '../../directives/MaterializeR/materialize-tabs.directive';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { SongType } from '../../models/songtype';
import { TypeService } from '../../services/type.service';
import { ButtonTransition } from '../../components/transitions/button.transition';
import { observable, computed } from 'mobx-angular';
import { RankingService } from 'src/app/services/ranking.service';
import { SongLogs } from 'src/app/models/songLogs';
import { debounceTime } from 'rxjs/operators';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-home-songs',
  templateUrl: './home-songs.component.html',
  styleUrls: ['./home-songs.component.css'],
  animations:[
    ButtonTransition
  ]
})
export class HomeSongsComponent implements OnInit, AfterViewInit, OnDestroy {
  private dayCount=14;
  private daySum=7;
  private songTop=5;


  @observable isDownloading=false;
  @observable isRequesting=false;
  @observable songList: Song[]=[];
  @observable songTypes: SongType[];
  rankColor=['ani-warn','ani-orange','ani-gold','ani-lime','ani-lightgreen']
  constructor(private songService: SongService, public player: MusicPlayerService,
    public lang: LanguageService,
    private loader: LoaderService,
    private user: UserService,
    private typeService: TypeService,
    private ranker: RankingService
  ) { }

  datasets: ChartDataset[]= [];
  scaling:string[];

  @ViewChild(MaterializeTabsDirective) tabs: MaterializeTabsDirective;
  @ViewChild(ChartJsDirective) chart: ChartJsDirective;

  ngOnInit() {
    this.setScaling();
    //this.fetch();
    if (this.user.IsAuthenticated)
      this.user.myProfile.subscribe(p=>{
        const profile=this.user.profile.getValue();
        profile.SongFavorite=p.SongFavorite;
        this.user.next(profile);
      })
  }

  ngAfterViewInit(){
    this.getType();
    this.getSummary();
  }

  ngOnDestroy(){

  }

  getSummary(){
    this.datasets=[];
    this.ranker.summary(this.daySum,this.songTop, this.dayCount+1).subscribe(res=>{
      if (res)
      res.forEach(summary=>{
        this.datasets.push(this.resolveDataset(summary.song.Name, summary.summary));
        this.songList.push(summary.song);
      });
      setTimeout((()=>{
        this.chart.setDatasets(this.datasets);
        //this.chart.update(20);
      }).bind(this),1200)
    },err =>{
    })
  }

  fetch(){
    this.loader.isSubLoading=true;
    this.songService.fetch().subscribe(val=>{
      this.songList=val;
      this.loader.isSubLoading=false;
      this.tabs.update();
    });
  }

  getType(){
    this.typeService.fetch().subscribe(res=>{
      this.songTypes=res;
    })
  }

  setScaling(){
    let date=new Date();
    date.setDate(date.getDate()-1);
    this.scaling=[];
    this.scaling.unshift((date.getDate())+'/'+(date.getMonth()+1));
    while (this.scaling.length<this.dayCount){
      date.setDate(date.getDate()-1);
      this.scaling.unshift((date.getDate())+'/'+(date.getMonth()+1));
    }
  }

  download(song: Song){
    if (!this.isDownloading){
      this.isDownloading=true;
      var sub=this.songService.download(song).subscribe(val=>{
        this.isDownloading=false;
        sub.unsubscribe();
      });
    }
  }

  favorite(song:Song){
    if (this.user.IsAuthenticated){
      this.isRequesting=true;
      this.songService.toggleFavorite(song).subscribe(val=>{
        this.isRequesting=false;
        const profile: User=this.user.profile.getValue();
        profile.SongFavorite=val;
        this.user.next(profile);
      },err=>{
        this.isRequesting=false;
        let msg=this.lang.ui.message_requestfail;
        if (err.status==0)
        {
          msg=this.lang.ui.message_connectfail;
        }
        MaterializeToastService.send(msg,"rounded red");
      })
    }
  }



  resolveDataset(title: string, data: SongLogs[]):ChartDataset{
    const arr: number[]=[];
    const date: Date=new Date();
    const baseDate=new Date(data[data.length-1].CreatedOn);
    arr.push(data[data.length-1].ViewCount);
    date.setDate(date.getDate()-this.dayCount+1);
    while (arr.length<this.dayCount)
    {
      if (!this.compareDate(date,baseDate)){
        const item=data.find(x=> this.compareDate(new Date(x.CreatedOn),date));
        if (item==null){
          if (arr.length>=1)
            arr.push(arr[arr.length-1]);
        }
        else {
          if (date.getTime()<baseDate.getTime())
            arr.unshift(item.ViewCount);
          else arr.push(item.ViewCount);
        }
      }
      date.setDate(date.getDate()+1);
    }
     return {
        label: title,
        data:arr,
        backgroundColor: ChartJsDirective.defaultFillColor[this.datasets.length],
        borderColor: ChartJsDirective.defaultColors[this.datasets.length],
        pointRadius:3,
        pointBackgroundColor:  ChartJsDirective.defaultColors[this.datasets.length],
        pointBorderColor: "white",
        pointBorderWidth: 2,
        fill:true,
      }
  }



  isFavorited(song: Song){
    return this.user.profile.getValue().SongFavorite!=null && this.user.profile.getValue().SongFavorite.includes(song.Id);
  }

  @computed get isLoggedIn(){
    return this.user.IsAuthenticated;
  }

  compareDate(source: Date, target: Date){
    return source.getDate()==target.getDate() &&
      source.getMonth()==target.getMonth() &&
      source.getFullYear()==target.getFullYear();
  }
}
