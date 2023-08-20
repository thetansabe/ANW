import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';
import { environment } from '../../../environments/environment';
import { LanguageService } from '../../services/language.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';

@Component({
  selector: 'app-admin-navbar',
  templateUrl: './admin-navbar.component.html',
  styleUrls: ['./admin-navbar.component.css']
})
export class AdminNavbarComponent implements OnInit {

  profile: User;
  constructor(
    private userService: UserService,
    public lang: LanguageService
  ) { }

  ngOnInit() {

    this.userService.profile.subscribe(res=>{
      this.profile=res;
    });
  }
  logout(){
    this.userService.logout();
    MaterializeToastService.send(this.lang.ui.loggedout);
  }

  get AvatarImg(){
    if (this.profile && this.profile.AvatarImg)
      return environment.RSRC_URL+this.profile.AvatarImg;
    return '../../../assets/images/icons/empty-user.png';
  }
}
