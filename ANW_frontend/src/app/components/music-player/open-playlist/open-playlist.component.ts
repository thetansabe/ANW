import { Component, OnInit, ViewChild, Input, ChangeDetectionStrategy } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { FormBuilder, FormGroup } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { PlaylistService } from '../../../services/playlist.service';
import { Playlist } from '../../../models/playlist';
import { MusicPlayerService } from '../../../services/music-player.service';
import { observable } from 'mobx-angular';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-open-playlist',
  templateUrl: './open-playlist.component.html',
  styleUrls: ['./open-playlist.component.css']
})
export class OpenPlaylistComponent implements OnInit {

  @observable form: FormGroup;
  @observable playlists: Playlist[];
  @observable isLoading=false;

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  constructor(
    public lang: LanguageService,
    private builder: FormBuilder,
    private playlistService: PlaylistService,
    public player: MusicPlayerService
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      filterInput:''
    });
    this.onSearch();
  }

  onSearch(){
    this.form.get('filterInput').valueChanges.pipe(
      debounceTime(500)
    ).subscribe(val=>{
      this.fetch(val);
    })
  }

  fetch(keyword: string=null){
    this.isLoading=true;
    if (keyword && keyword.length)
      this.playlistService.findmy(keyword).subscribe(res=>{
        this.playlists=res;
        this.isLoading=false;
      }, err=>{
        this.isLoading=false;
      });
    else
      this.playlistService.getmy(8).subscribe(res=>{
        this.playlists=res;
        this.isLoading=false;
      },err=>{
        this.isLoading=false;
      })
  }

  open(){
    this.modal.open();
    if (!this.playlists)
      this.fetch();
  }

  setPlaylist(playlist: Playlist){
    this.modal.close();
    //if (!playlist || (this.player.currentPlaylist && playlist.Id==this.player.currentPlaylist.Id)) return;
    this.playlistService.get(playlist.Id).subscribe(res=>{
      this.player.currentPlaylist=res.playlist;
      this.player.playlist=[];
      if (res.songlist && res.songlist.length>0){
        this.player.playlist=res.songlist.filter(x=>x);
        if (this.player.playlist.length>=1)
          this.player.playSong(this.player.playlist[0]);
        else this.player.pause();
      }
    }, err=>{
      if (err.status==400 || err.status==404){
        MaterializeToastService.send(this.lang.ui.message_notexists,"red rounded");
      }
    })
  }
}
