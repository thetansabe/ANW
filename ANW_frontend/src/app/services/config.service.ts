import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';
import { Observable, throwError } from 'rxjs';
import { pipe } from '@angular/core/src/render3';
import { SystemConfig } from '../models/systemConfig';
import { catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable()
export class ConfigService {

  constructor(
    private http: HttpClient,
    private userService: UserService,
    private router: Router
  ) { }

  get(){
    if (!this.userService.IsAuthenticated) 
    {
      this.router.navigate(["/login"]);
      return new Observable<SystemConfig>();
    }
    return this.http.get(this.URL,{headers: this.userService.getAuthorizedHeader() })
    .pipe(
      catchError(e=>throwError(e)),
      map(res => res as SystemConfig)
    )
  }

  save(config: SystemConfig){
    if (!this.userService.IsAuthenticated) 
    {
      this.router.navigate(["/login"]);
      return new Observable();
    }
    return this.http.put(this.URL,config,{headers: this.userService.getAuthorizedHeader() })
    .pipe(
      catchError(e=> throwError(e))
    );
  }

  get URL(){
    return environment.HOST+"/api/administrator/system";
  }
}
