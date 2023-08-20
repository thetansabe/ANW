import { Component, OnInit, ViewChild } from '@angular/core';
import { VideoService } from 'src/app/services/video.service';
import { LoaderService } from 'src/app/services/loader.service';
import { LanguageService } from 'src/app/services/language.service';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Video } from 'src/app/models/video';
import { UserService } from 'src/app/services/user.service';
import { debounceTime } from 'rxjs/operators';
import { environment } from 'src/environments/environment.prod';
import { MaterializeToastService } from 'src/app/directives/MaterializeR/materialize-toast.service';
import { VideoUploaderComponent } from './video-uploader/video-uploader.component';
import { VideoPopoutComponent } from 'src/app/components/video-popout/video-popout.component';

@Component({
  selector: 'app-video-manage',
  templateUrl: './video-manage.component.html',
  styleUrls: ['./video-manage.component.css']
})
export class VideoManageComponent implements OnInit {

  page: number;
  pages: number[];
  maxPage: number;
  private size: number=12;
  list: Video[];
  filterForm: FormGroup;
  isLoading: boolean=false;
  private q: string;
  checker: boolean[]=[];

  @ViewChild(VideoUploaderComponent) uploader: VideoUploaderComponent;
  @ViewChild(VideoPopoutComponent) player: VideoPopoutComponent;

  constructor(
    public lang: LanguageService,
    private video: VideoService, private loader: LoaderService,
    private user: UserService,
    private router: Router, private actRoute: ActivatedRoute,
    private builder: FormBuilder
  ) { }

  ngOnInit() {
    this.actRoute.queryParams.subscribe(params=>{
      if (params['page'])
        this.page=+params['page'];
      else this.page=1;
      if (params['size'])
        this.size=+params['size'];
      if (params['q'])
          this.q=params['q'];
      this.fetch();
    });
    this.filterForm= this.builder.group({
      filterInput:''
    });
    this.filterForm.get("filterInput").valueChanges.pipe(debounceTime(350)).subscribe(val=>{
      this.router.navigate(['.'], { relativeTo: this.actRoute, queryParams:{ page:this.page, size: this.size, q: val}});
    })
  }

  fetch(){
    this.loader.isSubLoading=true;
    this.video.filter(this.page,this.size, this.q).subscribe(res=>{
      this.loader.isSubLoading=false;
      this.list=res.list;
      this.maxPage=res.maxPage;
      this.pages=[];
      this.checker=[];
      console.log(this.list);
      for (var i=1; i<= res.maxPage; i++)
        this.pages.push(i);
    }, err=>{
      this.loader.isSubLoading=false;
      this.maxPage=0;
      this.list=[];
      if (err.status==403)
        this.user.logout();
    })
  }

  get isCheckedAll(){
    if (this.list && this.list.length>0)
    {
      if (this.list.length==this.checker.length)
        return this.checker.every(x=>x);
    }
    return false;
  }

  approve( level: number){
    var ids: string[]=[];
    for (var i=0; i< this.checker.length; i++)
    {
      if (this.checker[i] && this.list[i])
        ids.push(this.list[i].Id);
    }
    this.isLoading=true;
    if (ids.length>0)
      this.video.approve(ids,level).subscribe(res=>{
        this.isLoading=false;
        res.forEach(video=>{
          this.list.find(x=>x.Id==video.Id).Approve=video.Approve;
        })
        MaterializeToastService.send(this.lang.ui.message_requestsuccess,"rounded green");
      },err =>{
        this.isLoading=false;
        MaterializeToastService.send(this.lang.ui.message_requestfail,"rounded red");
      })
  }

  toggleCheck(){
    var checkedAll=this.isCheckedAll;
    for (var i=0; i< this.list.length; i++)
      this.checker[i]=!checkedAll;
  }

  get isChecked(){
    if (this.list && this.list.length>0)
    {
      return this.checker.length>0 && this.checker.some(x=>x);
    }
    return false;
  }

  get isAdmin(){
    return this.user.IsAuthenticated && this.user.getUserLevel()>=9;
  }

  getImageUrl(url: string)
  {
    if (url)
      return environment.MEDIA_URL+url;
    else
      return "../../../assets/images/bg.jpg";
  }

  afterUploaded(video: Video){
    if (this.list.length>=this.size){
      this.list.pop();
    }
    this.list.unshift(video);
  }

  upload(){
    this.uploader.open();
  }

  play(video: Video){
    this.player.open(video);
  }
}
