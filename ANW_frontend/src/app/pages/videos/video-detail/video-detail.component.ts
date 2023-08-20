import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { Video } from 'src/app/models/video';
import { VideoService } from 'src/app/services/video.service';
import { Router, ActivatedRoute } from '@angular/router';
import { environment } from 'src/environments/environment';
import { MusicPlayerService } from 'src/app/services/music-player.service';

@Component({
  selector: 'app-video-detail',
  templateUrl: './video-detail.component.html',
  styleUrls: ['./video-detail.component.css']
})
export class VideoDetailComponent implements OnInit, OnDestroy {

  currentVideo: Video;

  @ViewChild("video") videoPlayer: ElementRef;
  constructor(private video: VideoService, private router: Router, 
    private musicPlayer:MusicPlayerService,
    private actRoute:ActivatedRoute) { }

  ngOnInit() {
    this.actRoute.params.subscribe(params=>{
      if (params['id'])
        this.video.getById(params['id']).subscribe(res=>{
          this.currentVideo=res;
        }, err=>{
          this.router.navigate(["../.."], {relativeTo: this.actRoute});
        })
      else
        this.router.navigate(["../.."], {relativeTo: this.actRoute});
    });
    this.videoPlayer.nativeElement.onplay=(()=>{
      this.musicPlayer.controlDisabled=true;
      this.musicPlayer.pause();
    }).bind(this);
    this.videoPlayer.nativeElement.onpause=(()=>{
      this.musicPlayer.controlDisabled=false;
    }).bind(this);
  }
  ngOnDestroy(){
    this.musicPlayer.controlDisabled=false;
  }

  get URL(){
    if (this.currentVideo)
      return environment.HOST+"/api/video/play/"+this.currentVideo.Id;
    else return null;
  }
}
