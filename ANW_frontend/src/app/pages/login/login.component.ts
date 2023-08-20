import { Component, OnInit, AfterContentInit, ChangeDetectionStrategy, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { LanguageService } from '../../services/language.service';
import { trigger, transition, style,animate ,keyframes} from '@angular/animations'
import { UserService } from '../../services/user.service';
import { Router, ActivatedRoute } from '@angular/router';
import { User } from '../../models/user';
import { SignalrService } from '../../services/signalr.service';
import { Subscription } from '../../../../node_modules/rxjs';
import { observable } from '../../../../node_modules/mobx-angular';

declare var M: any;
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  animations:[
    trigger("transition",[
      transition(":enter",[
        style({ opacity: 0, transform:'scale(0.4)'}),
        animate("0.4s 200ms ease-in-out", keyframes([
          style({ opacity:0.8, transform:'scale(1.1)', offset:0.4}),
          style({  opacity:1, transform:'scale(1)', offset:1})
        ])
      )
    ])
  ])]
})
export class LoginComponent implements OnInit, AfterContentInit, OnDestroy {
  @observable username="";
  @observable password="";
  @observable remember=false;
  @observable isRequesting=false;
  returnRoute:string;
  returnUrl: string;
  @observable profileSub: Subscription;
  constructor(public lang: LanguageService, 
    private userService: UserService, 
    private router: Router,
    private actRoute: ActivatedRoute,
    private signalR: SignalrService,
    private watcher: ChangeDetectorRef
  ) { }

  ngOnInit() {
    if (this.userService.IsAuthenticated)
      this.router.navigate(['/account']);
    this.actRoute.queryParams.subscribe(params=>{
      if (params['returnRoute'])
        this.returnRoute=params['returnRoute'];
      if (params['returnUrl'])
        this.returnUrl=params['returnUrl'];
    })
  }
  ngAfterContentInit(){
    window.scrollTo(0,0);
    setTimeout(()=> window.scrollTo(0,0),300);
  }
  login(ev){
    this.watcher.detectChanges();
    ev.preventDefault();
    this.isRequesting=true;
    if (this.profileSub){
      this.profileSub.unsubscribe();
      this.profileSub=null;
    }
    var log=this.userService.login(this.username,this.password)
    .subscribe(result=>{
      this.isRequesting=false;
      this.profileSub=this.userService.profile.subscribe(val=>{
        if (val!=null && this.profileSub){
          if (this.profileSub){
            this.profileSub.unsubscribe();
            this.profileSub=null;
          }
          M.toast({html: '<span class="new badge orange" data-badge-caption="WELCOME!" style="margin-right: 12px;"></span>'+this.lang.ui.welcomelogin+' '+val.DisplayName, classes:'green darken-3 z-depth-0', displayLength: 6000});
        }
      });
      this.userService.SetAuthentication(result);
      if (this.returnUrl)
        this.router.navigateByUrl(this.returnUrl);
      else
      if (this.returnRoute)
        this.router.navigate([this.returnRoute]);
      else
        this.router.navigate(['/']);
      log.unsubscribe();
      this.signalR.sendAuthorize();
    },
    err=>{
      this.isRequesting=false;
      if (err.status==400)
        M.toast({html: this.lang.ui.authenfail, classes:'red rounded', displayLength: 6000});
      else 
        M.toast({html: this.lang.ui.loginfail, classes:'red rounded', displayLength: 6000});
      this.watcher.detectChanges();
    });
  }

  welcome(data: User){
    
  }

  ngOnDestroy(){
    this.watcher.detach();
  }
}
