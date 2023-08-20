import { Component, OnInit, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { MenuItem,  DashboardItemList } from '../menu-items/menu-items.model';
import { LanguageService } from 'src/app/services/language.service';
import { UserService } from '../../services/user.service';
import { Subscription } from '../../../../node_modules/rxjs';

declare var M:any;
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-side-menu',
  templateUrl: './side-menu.component.html',
  styleUrls: ['./side-menu.component.css']
})
export class SideMenuComponent implements OnInit, AfterViewInit, OnDestroy {

  private subscription: Subscription;
  constructor(
    public lang: LanguageService,
    private userService: UserService,
    private watcher: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.subscription=this.userService.LoginState.subscribe(()=>{
      this.watcher.detectChanges();
    })
  }

  ngAfterViewInit()
  {
    var ele=document.getElementById("sidenav-admin");
    var instance=M.Sidenav.init(ele, { preventScrolling: false });
  }

  get itemList(): MenuItem[]{
    return DashboardItemList.filter(x=>x.route && (!x.displayLevel || x.displayLevel<this.userService.getUserLevel()));
  }
  ngOnDestroy(){
    if (this.subscription)
      this.subscription.unsubscribe();
    this.watcher.detach();
  }
}
