import { Component, OnInit, ChangeDetectionStrategy, Input, ViewChild, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { Song } from '../../../models/song';
import { observable, computed } from 'mobx-angular';
import { SongManagerService } from '../../../services/song-manager.service';
import { LanguageService } from '../../../services/language.service';
import { TypeService } from '../../../services/type.service';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { SongType } from '../../../models/songtype';
import { MaterializeChipsDirective, materializeChip } from '../../../directives/MaterializeR/materialize-chips.directive';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-edit-song',
  templateUrl: './edit-song.component.html',
  styleUrls: ['./edit-song.component.css']
})
export class EditSongComponent implements OnInit {
  @observable isLoading: boolean=false;
  @observable selectedSong: Song;

  @observable songTypes: SongType[]=[];
  @observable songName: string;
  @observable mysong: boolean;
  @observable songType: string;

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  @ViewChild(MaterializeChipsDirective) chip: MaterializeChipsDirective;
  @Output() onChanged: EventEmitter<Song>=new EventEmitter<Song>();
  constructor(
    private songManager: SongManagerService,
    public lang: LanguageService,
    private typeService: TypeService,
    private detector: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.typeService.fetch().subscribe((res)=>{
      this.songTypes=res;
    });
  }

  submit(){
    const artists: string[]=[];
    const inputArtist=this.chip.values();
    for (var i=0; i<inputArtist.length; i++){
      const value=(inputArtist[i].tag+'').trim();
      if (value && value.length>0)
        artists.push(value);
    }
    this.isLoading=true;
    this.songManager.upload(this.songName,artists,this.songType,null,this.mysong?"true":null,this.selectedSong.Id)
    .subscribe(val=>{
      this.modal.close();
      this.isLoading=false;
      this.selectedSong.Name=this.songName;
      this.selectedSong.SubType=this.songType;
      this.selectedSong.SelfPerformance=this.mysong?1:0;
      this.onChanged.emit(this.selectedSong);
      MaterializeToastService.send(this.lang.ui.message_requestsuccess,"green rounded");
    },err=>{
      this.isLoading=false;
      let msg="";
      if (err.status==403)
        msg=this.lang.ui.message_forbid;
      else msg=this.lang.ui.message_requestfail;
      MaterializeToastService.send(msg,"red rounded");
    });
  }

  open(song: Song, artistList: any=null){
    this.songType=null;
    this.selectedSong=song;
    this.modal.open();
    this.songName=song.Name;
    this.mysong=song.SelfPerformance>0;
    const self=this;
    setTimeout((()=>{
      this.songType=song.SubType || " ";
      this.detector.detectChanges();
      console.log(this.songType);
    }).bind(this),150);
    this.chip.clear();
    if (this.selectedSong.Artists)
      this.selectedSong.Artists.forEach(artist=>{
        const item: materializeChip=new materializeChip();
        item.tag=artistList[artist].Name;
        item.image=artistList[artist].AvatarImg?this.url+artistList[artist].AvatarImg:null;
        this.chip.add(item);
      })
  }

  @computed get url(){
    return environment.MEDIA_URL;
  }
}
