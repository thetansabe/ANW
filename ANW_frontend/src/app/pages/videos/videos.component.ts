import { Component, OnInit, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { VideoService } from 'src/app/services/video.service';
import { Video } from 'src/app/models/video';
import { observable } from 'mobx-angular';
import { environment } from 'src/environments/environment';
import { VideoPopoutComponent } from 'src/app/components/video-popout/video-popout.component';
import { LoaderService } from 'src/app/services/loader.service';
import { load } from '@angular/core/src/render3';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-videos',
  templateUrl: './videos.component.html',
  styleUrls: ['./videos.component.css']
})
export class VideosComponent implements OnInit {

  @observable list: Video[];

  constructor(
    private video: VideoService,
    private loader: LoaderService
  ) { }

  ngOnInit() {
    this.loader.isSubLoading=true;
    this.video.get(12).subscribe(res=>{
      this.list=res.list;
      this.loader.isSubLoading=false;
    })
  }

  getImageUrl(url:string){
    if (url)
      return environment.MEDIA_URL+url;
    return '../../../assets/images/bg.jpg';
  }

}
