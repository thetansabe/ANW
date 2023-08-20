import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import {environment} from '../../../environments/environment';
import { catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-administrator-board',
  templateUrl: './administrator-board.component.html',
  styleUrls: ['./administrator-board.component.css']
})
export class AdministratorBoardComponent implements OnInit {

  initSuccess=false;
  countdown: number;
  constructor(private http: HttpClient, private router: Router, private userService: UserService) { }

  ngOnInit() {
    let option={};
    if (this.userService.IsAuthenticated)
      option={headers: this.userService.getAuthorizedHeader()};
    this.http.post(environment.HOST+"/api/administrator/system/inituser","", option).pipe(
      catchError((err)=>{
        return throwError(err);
      })
    ).subscribe(()=>{
      this.initSuccess=true;
      this.countdown=10;
      const timer=setInterval((()=>{
        this.countdown--;
        if (this.countdown==0){
          clearInterval(timer);
          this.router.navigate(['/']);
        }
      }).bind(this),1100)
    },err=>{
      this.router.navigate(['/notfound']);
    })
  }

}
