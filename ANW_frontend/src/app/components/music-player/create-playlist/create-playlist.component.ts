import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { MusicPlayerService } from '../../../services/music-player.service';
import { Playlist } from '../../../models/playlist';
import { PlaylistService } from '../../../services/playlist.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';

@Component({
  selector: 'app-create-playlist',
  templateUrl: './create-playlist.component.html',
  styleUrls: ['./create-playlist.component.css']
})
export class CreatePlaylistComponent implements OnInit {

  playlistName: string;
  publicMode: number=1;
  isRequesting=false;

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  constructor(public lang: LanguageService, private player: MusicPlayerService, private playlist: PlaylistService) { }

  ngOnInit() {
  }

  submit(){
    const playlist: Playlist=new Playlist();
    playlist.Name=this.playlistName;
    playlist.Public=this.publicMode;
    playlist.Collection=[];
    for (var i=0; i< this.player.playlist.length;i++)
      playlist.Collection.push(this.player.playlist[i].Id);
    this.isRequesting=true;
    this.playlist.create(playlist).subscribe(result=>{
      this.isRequesting=false;
      this.close();
      this.player.currentPlaylist=result;
      this.player.isAltered=false;
      MaterializeToastService.send(this.lang.ui.message_requestsuccess,"rounded green");
    },err=>{
      this.isRequesting=false;
      let msg=this.lang.ui.message_requestfail;
      if (err.status==401)
        msg=this.lang.ui.message_unauthorized;
      MaterializeToastService.send(msg,"rounded red");
    })
  }

  open(){
    this.modal.open();
  }
  close(){
    this.modal.close();
  }
}
