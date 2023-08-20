import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { Song } from '../../../models/song';
import { LanguageService } from '../../../services/language.service';
import { SongManagerService } from '../../../services/song-manager.service';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { Router } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';

@Component({
  selector: 'app-delete-song',
  templateUrl: './delete-song.component.html',
  styleUrls: ['./delete-song.component.css']
})
export class DeleteSongComponent implements OnInit {

  @Input() selectedSong: Song;
  @Input() selectedSongList: Song[];
  @Output() reloader: EventEmitter<any>=new EventEmitter();
  
  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  constructor(public lang: LanguageService,
    private songManager: SongManagerService,
    private router: Router,
    private userService: UserService
  ) { }

  ngOnInit() {
  }


  remove(song:Song){
    const indx=this.selectedSongList.findIndex(x=>x.Id==song.Id);
    if (indx>=0)
      this.selectedSongList.splice(indx,1);
    if (this.selectedSongList.length==0) this.modal.close();
  }

  submit(){
    if (this.selectedSong){
      this.songManager.delete(this.selectedSong.Id)
      .subscribe(res=>{
        MaterializeToastService.send(this.selectedSong.Name+' '+this.lang.ui.message_songdeleted);
        this.modal.close();
        if (this.reloader)
          this.reloader.emit('');
      }, err=>{
        if (err.status==401 || err.status==403){
          MaterializeToastService.send(this.lang.ui.message_unauthorized,"rounded red");
          this.modal.close();
          if (err.status==401)
            this.userService.logout();
        }
        else{
          MaterializeToastService.send(this.lang.ui.message_songcantdelete,"rounded red");
        }
      })
    }
    else if (this.selectedSongList && this.selectedSongList.length){
      let list: string[]=[];
      for (var i=0; i < this.selectedSongList.length; i++)
        list.push(this.selectedSongList[i].Id);
      this.songManager.batchDelete(list)
      .subscribe(res=>{
        MaterializeToastService.send(this.lang.ui.message_deletedone);
        this.modal.close();
        if (this.reloader)
          this.reloader.emit('');
      }, err=>{
        if (err.status==401 || err.status==403){
          MaterializeToastService.send(this.lang.ui.message_unauthorized,"rounded red");
          this.modal.close();
          if (err.status==401)
            this.userService.logout();
        }
        else{
          MaterializeToastService.send(this.lang.ui.message_songcantdelete,"rounded red");
        }
      })
    }
  }
}
