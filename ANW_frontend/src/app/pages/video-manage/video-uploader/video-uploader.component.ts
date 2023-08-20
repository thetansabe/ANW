import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { UserService } from 'src/app/services/user.service';
import { Router, ActivatedRoute } from '@angular/router';
import { VideoService } from 'src/app/services/video.service';
import { LanguageService } from 'src/app/services/language.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MaterializeChipsDirective } from 'src/app/directives/MaterializeR/materialize-chips.directive';
import { TypeService } from 'src/app/services/type.service';
import { SongType } from 'src/app/models/songtype';
import { MaterializeModalDirective } from 'src/app/directives/MaterializeR/materialize-modal.directive';
import { MaterializeToastService } from 'src/app/directives/MaterializeR/materialize-toast.service';
import { Video } from 'src/app/models/video';

@Component({
  selector: 'app-video-uploader',
  templateUrl: './video-uploader.component.html',
  styleUrls: ['./video-uploader.component.css']
})
export class VideoUploaderComponent implements OnInit {

  videoFile:File;
  isLoading: boolean=false;

  form: FormGroup;
  songTypes: SongType[];

  @Output() onUploaded: EventEmitter<Video>= new EventEmitter<Video>();
  @ViewChild(MaterializeChipsDirective) chips: MaterializeChipsDirective;
  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;

  constructor(
    public lang: LanguageService,
    private user: UserService, private router: Router,
    private actRoute: ActivatedRoute, private video: VideoService,
    private type: TypeService,
    private builder: FormBuilder
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      videoFile: ['', Validators.required],
      thumbnailFile:['', Validators.required],
      name:['', Validators.required],
      type:''
    });
    this.type.fetch().subscribe(val=>{
      this.form.controls['type'].setValue(val[0].SubType[0].Id);
      this.songTypes=val;
    });
  }

  setVideoFile(event){
    if (event.target.files)
    {
      this.videoFile=event.target.files[0];
    }
  }
  submit(){
    var artists: string[]=[];
    const inputArtist=this.chips.values();
    for (var i=0; i<inputArtist.length; i++){
      const value=(inputArtist[i].tag+'').trim();
      if (value && value.length>0)
        artists.push(value);
    }
    var name=this.form.get("name").value;
    var type=this.form.get("type").value;
    this.video.upload(name,artists,type,this.videoFile).subscribe(res=>{
      this.modal.close();
      this.onUploaded.emit(res);
      MaterializeToastService.send(this.lang.ui.message_requestsuccess,"rounded green");
    }, err=>{
      MaterializeToastService.send(this.lang.ui.message_requestfail, "rounded red");
      if (err.status==403)
        this.user.logout();
    });
  }

  open()
  {
    this.form.get("name").setValue("");
    this.chips.clear();
    this.form.controls['type'].setValue(this.songTypes[0].SubType[0].Id);
    this.modal.open();
  }
}
