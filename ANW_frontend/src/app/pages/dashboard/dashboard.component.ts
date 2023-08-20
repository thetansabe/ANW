import { Component, OnInit, AfterContentInit, ChangeDetectionStrategy } from '@angular/core';
import { LanguageService } from '../../services/language.service';
import { trigger, transition, query, style, stagger, animate} from '@angular/animations';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { DashboardItemList, MenuItem } from '../../components/menu-items/menu-items.model';
import { observable } from 'mobx-angular';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  animations:[
    trigger('transition',[
      //Begin Trans
      transition(':enter',[
        // Query
        query('.dashboard-item',style({ opacity: 0, transform:'translateX(80px)'})),
        query('.dashboard-item', stagger(80,[
          // Stagger
          animate("0.15s 150ms ease-in-out", style({ opacity:1, transform:'translateX(0)'}))
          //End Stagger
        ]))
        // End Query
      ])
      //End trans
    ])
  ]
})
export class DashboardComponent implements OnInit , AfterContentInit{
  @observable profile: User;
  @observable level: number;
  list:MenuItem[]=DashboardItemList;
  @observable displayList: MenuItem[]=[];
  constructor(public lang: LanguageService, private userService:UserService) { }

  ngOnInit() {
    this.userService.profile.subscribe(val=>{
      if (val){
        this.profile=val;
        this.level=this.userService.getUserLevel();
        //console.log(this.level);
        this.displayList=this.list.filter(x=>!x.hideOnDashboard && (!x.displayLevel || x.displayLevel <= this.level) && (!x.displayUntil || this.level<=x.displayUntil));
      }
    });
  }

  ngAfterContentInit(){
    window.scrollTo(0,0);
    setTimeout(()=> window.scrollTo(0,0),300);
  }
}
