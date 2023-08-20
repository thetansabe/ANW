import { Component, OnInit, AfterViewInit, AfterContentInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { UserService } from '../../services/user.service';
import { LanguageService } from '../../services/language.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { computed, observable } from 'mobx-angular';
import { Subscription } from '../../../../node_modules/rxjs';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit, AfterContentInit,OnDestroy {
  @observable navExpanded=false;

  @observable searchForm: FormGroup;

  private minOffset=40;

  private subscription: Subscription;
  constructor(private user: UserService,public lang: LanguageService,
    private builder: FormBuilder,
    private router: Router,
    private watcher: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.searchForm=this.builder.group({
      searchInput:['', Validators.required]
    })
  }

  ngAfterContentInit(){
    this.subscription= this.user.LoginState.subscribe((val)=>{
      this.watcher.detectChanges();
    })
    const offset=window.scrollY;
    this.navExpanded=offset>=this.minOffset;
    window.addEventListener('scroll',(()=>{
      
      if (this.watcher){
        const offset=window.scrollY;
        if (offset>=this.minOffset)
        {
          if (this.navExpanded==false){
            this.navExpanded=true;
              this.watcher.detectChanges();
          } 
        }
        else{
          if (this.navExpanded){
            this.navExpanded=false;
              this.watcher.detectChanges();
          }
        }
      }
    }).bind(this));
  }

  onSearchPost(){
    if (this.searchForm.valid){
      this.router.navigate(['/songs/search'],{ queryParams:{
        keyword: this.searchForm.get('searchInput').value,
        page: 1,
        size: 12
      }});
    }
  }

  get isAuthenticated(){
    return this.user.IsAuthenticated;
  }
  get getDisplayname(){
    if (this.user.profile.getValue())
      return this.user.profile.getValue().DisplayName;
    return "";
  }
  logout(){
    this.user.logout();
    this.watcher.detectChanges();
    MaterializeToastService.send(this.lang.ui.loggedout);
  }
  get isAdmin(){
    return this.user.IsAuthenticated && this.user.getUserLevel()>=9;
  }

  ngOnDestroy(){
    if (this.subscription)
      this.subscription.unsubscribe();
    this.watcher.detach();
    this.watcher=null;
  }
}
