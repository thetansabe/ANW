import { Component, OnInit,  ViewChild, ElementRef } from '@angular/core';
import { LanguageService } from '../../services/language.service';
import { UserService } from '../../services/user.service';


declare var M:any;
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  @ViewChild('navBar') navBar: ElementRef;
  constructor(public lang: LanguageService,
    private user: UserService
  ) { }

  ngOnInit() {
  }

  get isAuthenticated(){
    return this.user.IsAuthenticated;
  }

  get isAdmin(){
    return this.user.IsAuthenticated && this.user.getUserLevel()>=9;
  }

  getDisplayname(){
    if (this.user.profile.getValue())
      return this.user.profile.getValue().DisplayName;
    return "";
  }

  logout(){
    this.user.logout();
  }
}
