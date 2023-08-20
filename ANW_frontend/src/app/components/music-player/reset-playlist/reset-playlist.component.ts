import { Component, OnInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { MusicPlayerService } from '../../../services/music-player.service';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-reset-playlist',
  templateUrl: './reset-playlist.component.html',
  styleUrls: ['./reset-playlist.component.css']
})
export class ResetPlaylistComponent implements OnInit {

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  constructor(
    public lang: LanguageService,
    private player: MusicPlayerService

  ) { }

  ngOnInit() {
  }

  reset(){
    this.player.playlist=[];
    this.player.currentPlaylist=null;
    this.player.isAltered=false;
    if (this.player.isPlaying)
      this.player.pause();
    this.modal.close();
  }

  open(){
    this.modal.open();
  }
}
