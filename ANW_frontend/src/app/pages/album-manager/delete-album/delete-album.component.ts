import { Component, OnInit, ChangeDetectionStrategy, ViewChild, Output, EventEmitter } from '@angular/core';
import { LanguageService } from 'src/app/services/language.service';
import { observable } from 'mobx-angular';
import { Album } from 'src/app/models/album';
import { MaterializeModalDirective } from 'src/app/directives/MaterializeR/materialize-modal.directive';
import { AlbumManagerService } from 'src/app/services/album-manager.service';
import { MaterializeToastService } from 'src/app/directives/MaterializeR/materialize-toast.service';
import { Router } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-delete-album',
  templateUrl: './delete-album.component.html',
  styleUrls: ['./delete-album.component.css']
})
export class DeleteAlbumComponent implements OnInit {

  @observable isLoading: boolean=false;
  @observable list: Album[];

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  @Output() onDeleted: EventEmitter<Album[]>=new EventEmitter<Album[]>();
  constructor(
    public lang:LanguageService,
    private albumManager: AlbumManagerService,
    private router: Router
  ) { }

  ngOnInit() {
  }

  delete(){
    this.isLoading=true;
    this.albumManager.batchDelete(this.list).subscribe((res)=>{
      this.isLoading=false;
      this.modal.close();
      MaterializeToastService.send(this.lang.ui.message_requestsuccess, "green rounded");
      const list=this.list.filter(x=> res.includes(x.Id));
      this.onDeleted.emit(list);
      this.list=[];
    },err =>{
      this.isLoading=false;
      let msg=this.lang.ui.message_deleteunable;
      if (err.status==401){
        msg=this.lang.ui.message_forbid;
      }
      else if (err.status==403){
        this.router.navigate(['/login']);
      }
      MaterializeToastService.send(msg,"red rounded");
    })
  }

  remove(target: Album){
    const indx=this.list.findIndex(x=>x.Id==target.Id);
    if (indx>=0)
      this.list.splice(indx,1);
    if (this.list.length==0) this.modal.close();
  }

  open(list: Album[]){
    this.list=list;
    this.modal.open();
  }
}
