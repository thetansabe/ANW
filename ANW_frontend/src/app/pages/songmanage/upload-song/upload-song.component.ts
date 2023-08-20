import { Component, OnInit, ViewChild, HostBinding, ChangeDetectionStrategy } from '@angular/core';
import { SimpleTransition } from '../../../components/transitions/simple.transition';
import { LanguageService } from '../../../services/language.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MaterializeChipsDirective } from '../../../directives/MaterializeR/materialize-chips.directive';
import { SongType } from '../../../models/songtype';
import { SongService } from '../../../services/song.service';
import { SongManagerService } from '../../../services/song-manager.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { Router, ActivatedRoute } from '@angular/router';
import { TypeService } from '../../../services/type.service';
import { observable } from 'mobx-angular';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-upload-song',
  templateUrl: './upload-song.component.html',
  styleUrls: ['./upload-song.component.css'],
  animations:[
    SimpleTransition
  ]
})
export class UploadSongComponent implements OnInit {
  @observable isUploading=false;
  form: FormGroup;
  songTypes: SongType[];
  uploadFile: File;

  @ViewChild(MaterializeChipsDirective) artists: MaterializeChipsDirective;
  constructor(public lang: LanguageService, 
    private builder: FormBuilder,
    private song: SongService, 
    private songManager: SongManagerService,
    private router: Router,
    private typeService: TypeService,
    private actRoute: ActivatedRoute
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      songname:['', Validators.required],
      file:['', Validators.required],
      mysong:'',
      songtype:''
    })
    this.typeService.fetch().subscribe(val=>{
      this.form.controls['songtype'].setValue(val[0].SubType[0].Id);
      this.songTypes=val;
    });
  }
  setFile(event){
    if (event.target.files)
    {
      this.uploadFile=event.target.files[0];
    }
  }
  upload(){
    if (!this.uploadFile) return;
    if (!this.uploadFile.type.includes("audio/")) return;
    this.isUploading=true;
    let artist=[];
    const inputArtist=this.artists.values();
    for (var i=0; i<inputArtist.length; i++){
      const value=(inputArtist[i].tag+'').trim();
      if (value && value.length>0)
        artist.push(value);
    }
    const name=this.form.get('songname').value;
    const type=this.form.get('songtype').value;
    const mysong=this.form.get('mysong').value
    const file:File=this.uploadFile;
    
    this.songManager.upload(name,artist,type,file,mysong).subscribe(res=>{
      MaterializeToastService.send(res.Name+' '+this.lang.ui.message_songuploaded);
      this.router.navigate(['../'], {relativeTo: this.actRoute});
      this.isUploading=false;
    },err=>{
      this.isUploading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail, 'rounded red');
    });
  }

}
