import { Injectable } from '@angular/core';
import { HttpClient } from '../../../node_modules/@angular/common/http';
import { environment } from '../../environments/environment';
import { pipe, throwError } from '../../../node_modules/rxjs';
import { catchError, map } from '../../../node_modules/rxjs/operators';
import { Nation } from '../models/nation';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class NationService {

  private URL: string= environment.HOST+"/api/nation";
  constructor(
    private http: HttpClient,
    private userService: UserService
  ) { }

  get(){
    return this.http.get(this.URL)
    .pipe(
      catchError(e=> throwError(e)),
      map( res=> res as Nation[])
    );
  }

  save(obj: Nation){
    return this.http.post(this.URL, obj, {headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(e=>throwError(e)),
      map(res => res as Nation)
    )
  }

  delete(obj: Nation){
    return this.http.delete(this.URL+"/"+ obj.Id, {headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(e=> throwError(e))
    )
  }
}
