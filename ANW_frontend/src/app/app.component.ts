import { Component, OnInit, ViewChild } from '@angular/core';
import { LanguageService } from './services/language.service';
import { Router, NavigationCancel,NavigationEnd,NavigationStart } from '@angular/router';
import { LoaderService } from './services/loader.service';
import { UserService } from './services/user.service';
import { User } from './models/user';
import { MaterializeSidenavDirective } from './directives/MaterializeR/materialize-sidenav.directive';
import { SignalrService } from './services/signalr.service';
import { MaterializeToastService } from './directives/MaterializeR/materialize-toast.service';


declare var M:any;
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  profile: User;
  previousState: string;
  @ViewChild(MaterializeSidenavDirective) sidenav: MaterializeSidenavDirective;
  constructor(public lang: LanguageService, 
    private userService: UserService, private router: Router, 
    public loader: LoaderService,
    private hub: SignalrService
  ){
  }

  get isAuthenticated(){
    return this.userService.IsAuthenticated;
  }

  logout(){
    this.userService.logout();
    M.toast({html: this.lang.ui.loggedout, classes:'rounded'});
  }

  ngOnInit(){
    this.router.events.subscribe((event)=>{
      if (event instanceof NavigationStart){
        if (this.router.url!=this.previousState) this.loader.isLoading=true;
        if (this.sidenav) this.sidenav.close();
      }
        else if (event instanceof NavigationCancel || event instanceof NavigationEnd){
          if (this.router.url!=this.previousState) {this.loader.isLoading=false;
          }
          this.previousState=this.router.url;
        }
    });
    this.userService.profile.subscribe(val=>{
      if (val){
        this.profile=val;
      }
    });

    this.hub.on("onWarning")
    .subscribe(message=>{
      if (message){
        const msg=message+'';
        MaterializeToastService.send("<i class='material-icons left'>warning</i>"+msg,"");
      }
    });
  }

  state(){
    return this.router.url;
  }
}
