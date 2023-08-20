import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { Video } from 'src/app/models/video';
import { environment } from 'src/environments/environment';
import { MusicPlayerService } from 'src/app/services/music-player.service';

@Component({
  selector: 'app-video-popout',
  templateUrl: './video-popout.component.html',
  styleUrls: ['./video-popout.component.css']
})
export class VideoPopoutComponent implements OnInit {

  src: string;
  isOpen: boolean=false;
  currentVideo: Video;

  @ViewChild("video") videoPlayer:ElementRef;

  constructor(
    private musicPlayer:MusicPlayerService
  ) { }

  ngOnInit() {
  }
 
  open(video: Video){
    this.src=environment.HOST+"/api/video/play/"+video.Id;
    this.isOpen=true;
    this.currentVideo=video;
    if (this.musicPlayer.isPlaying)
      this.musicPlayer.pause();
  }
  close(){
    this.videoPlayer.nativeElement.pause();
    this.isOpen=false;
  }
}
