import { Component, OnInit, ViewChild, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { Playlist } from '../../../models/playlist';
import { PlaylistService } from '../../../services/playlist.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { LanguageService } from '../../../services/language.service';


@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-delete-playlist',
  templateUrl: './delete-playlist.component.html',
  styleUrls: ['./delete-playlist.component.css']
})
export class DeletePlaylistComponent implements OnInit {

  playlist: Playlist;
  isLoading=false;

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  @Output("onDeleted") deleteCallback: EventEmitter<Playlist>=new EventEmitter<Playlist>();

  constructor(private playlistService: PlaylistService,
    public lang: LanguageService,
    private detector: ChangeDetectorRef
  ) { }

  ngOnInit() {
  }

  delete(){
    this.isLoading=true;
    this.detector.detectChanges();
    this.playlistService.delete(this.playlist.Id)
    .subscribe(()=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_deletedone,"rounded green");
      this.modal.close();
      if (this.deleteCallback)
        this.deleteCallback.emit(this.playlist);
      this.detector.detectChanges();
    },err=>{
      this.isLoading=false;
      this.detector.detectChanges();
      if (err.status==403){
        MaterializeToastService.send(this.lang.ui.message_forbid,"rounded red");
      }
      else if (err.status==403)
        MaterializeToastService.send(this.lang.ui.message_unauthorized,"rounded red");
      else
        MaterializeToastService.send(this.lang.ui.message_deleteunable,"rounded red");
    })
  }

  open(playlist: Playlist){
    this.playlist=playlist;
    this.modal.open();
    this.detector.detectChanges();
  }
}
