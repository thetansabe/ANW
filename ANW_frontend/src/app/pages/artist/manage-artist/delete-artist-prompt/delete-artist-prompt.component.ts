import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Artist } from '../../../../models/artist';
import { LanguageService } from '../../../../services/language.service';
import { ArtistService } from '../../../../services/artist.service';
import { MaterializeModalDirective } from '../../../../directives/MaterializeR/materialize-modal.directive';
import { MaterializeToastService } from '../../../../directives/MaterializeR/materialize-toast.service';

@Component({
  selector: 'delete-artist-prompt',
  templateUrl: './delete-artist-prompt.component.html',
  styleUrls: ['./delete-artist-prompt.component.css']
})
export class DeleteArtistPromptComponent implements OnInit {

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  @Output("onDeleted") deleteCallback: EventEmitter<Artist[]>= new EventEmitter<Artist[]>(); 
  isLoading=false;
  selectedArtists:Artist[];
  constructor(
    public lang: LanguageService,
    private artistService: ArtistService
  ) { }

  ngOnInit() {
  }

  open(list: Artist[])
  {
    if (list && list.length>0)
    {
      this.selectedArtists=list;
      this.modal.open();
    }
  }

  confirm(){
    this.isLoading=true;
    this.artistService.batchDelete(this.selectedArtists).subscribe(val=>{
      this.isLoading=false;
      if (val  && val.length>0){
        this.modal.close();
        if (this.deleteCallback)
          this.deleteCallback.emit(val);
      }
      else
        MaterializeToastService.send(this.lang.ui.message_deleteunable,"red rounded");
    }, err=>{
      this.isLoading=false;
      let msg: string;
      if (err.status==401)
        msg=this.lang.ui.message_unauthorized;
      else if (err.status==403)
        msg=this.lang.ui.message_forbid;
      else
        msg=this.lang.ui.message_requestfail;

      if (msg)
        MaterializeToastService.send(msg,"red rounded");
    })
  }
}
