import { Component, OnInit, ChangeDetectionStrategy, ViewChild, Output, EventEmitter } from '@angular/core';
import { LanguageService } from '../../../../services/language.service';
import { UserService } from '../../../../services/user.service';
import { ArtistService } from '../../../../services/artist.service';
import { Artist } from '../../../../models/artist';
import { observable, computed } from 'mobx-angular';
import { MaterializeModalDirective } from '../../../../directives/MaterializeR/materialize-modal.directive';
import { MaterializeToastService } from '../../../../directives/MaterializeR/materialize-toast.service';
import { environment } from '../../../../../environments/environment';
import { NationService } from '../../../../services/nation.service';
import { Nation } from '../../../../models/nation';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-edit-artist',
  templateUrl: './edit-artist.component.html',
  styleUrls: ['./edit-artist.component.css']
})
export class EditArtistComponent implements OnInit {
  
  @observable selectedArtist: Artist;

  @observable isLoading: boolean=false;
  @observable artistName: string;
  @observable dob: Date;
  @observable nation: string;
  @observable nations: Nation[];

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;

  @Output() onChanged: EventEmitter<Artist>=new EventEmitter<Artist>();
  constructor(
    public lang: LanguageService,
    private user: UserService,
    private artistService: ArtistService,
    private nationService: NationService
  ) { }

  ngOnInit() {
    this.nationService.get().subscribe(res=>{
      this.nations=res;
    })
  }

  open(artist: Artist){
    this.selectedArtist=artist;
    this.nation=null;
    
    setTimeout((()=>{
      this.nation= this.selectedArtist.Country || "";
    }).bind(this),120);
    this.modal.open();
  }
  uploadAvatar(e){
    if (!this.selectedArtist) return;
    const file=e.target.files[0];
    if (!file) return;
    this.isLoading=true;
    this.artistService.uploadAvatar(this.selectedArtist.Id, file)
    .subscribe(path=>{
      this.isLoading=false;
      this.selectedArtist.AvatarImg=path;
      e.target.value="";
    },err=>{
      this.isLoading=false;
      let msg="";
      if (err.status==403)
        msg=this.lang.ui.message_forbid;
      else msg=this.lang.ui.message_requestfail;
      MaterializeToastService.send(msg,"red rounded");
    })
  }
  uploadBackground(e){
    if (!this.selectedArtist) return;
    const file=e.target.files[0];
    if (!file) return;

    this.isLoading=true;
    this.artistService.uploadBackground(this.selectedArtist.Id, file)
    .subscribe(path=>{
      this.isLoading=false;
      this.selectedArtist.BackgroundImg=path;
      e.target.value="";
    },err=>{
      this.isLoading=false;
      let msg="";
      if (err.status==403)
        msg=this.lang.ui.message_forbid;
      else msg=this.lang.ui.message_requestfail;
      MaterializeToastService.send(msg,"red rounded");
    })
  }

  save(){
    this.isLoading=true;
    if (this.nation)
      this.selectedArtist.Country=this.nation;
    this.artistService.save(this.selectedArtist).subscribe(res=>{
      this.isLoading=false;
      this.modal.close();
      this.nation=null;
      this.selectedArtist.Name=res.Name;
      this.selectedArtist.DateOfBirth=res.DateOfBirth;
      this.selectedArtist.Country=res.Country;
      this.selectedArtist.CountryName=res.CountryName;
      this.selectedArtist.Desc=res.Desc;
      
      this.onChanged.emit(this.selectedArtist);

      MaterializeToastService.send(this.lang.ui.message_requestsuccess,"green rounded");
    },err=>{
      let msg="";
      this.isLoading=false;
      if (err.status==403)
        msg=this.lang.ui.message_forbid;
      else msg=this.lang.ui.message_requestfail;
      MaterializeToastService.send(msg,"red rounded");
    })
  }

  @computed get url(){
    return environment.MEDIA_URL;
  }
}
