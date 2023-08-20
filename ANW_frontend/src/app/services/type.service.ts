import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { catchError, map } from '../../../node_modules/rxjs/operators';
import { throwError, Observable } from '../../../node_modules/rxjs';
import { SongType, SongSubType } from '../models/songtype';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class TypeService {
  private URL:string =environment.HOST+"/api/type";

  constructor(
    private http: HttpClient, private userService: UserService
  ) { }

  fetch(): Observable<SongType[]>{
    return this.http.post(this.URL,"").pipe(
      catchError((err)=> throwError(err) ),
      map(res=> res as SongType[])
    );
  }

  add(type: SongType): Observable<SongType>{
    return this.http.post(this.URL+"/create", 
    type, {headers: this.userService.getAuthorizedHeader()}
    ).pipe(
      catchError(err=> throwError(err)),
      map(res=>res as SongType)
    );
  }

  addSubType(type: SongType, subtype: SongSubType): Observable<SongType>{
    return this.http.post(this.URL+"/append/"+type.Id, 
    subtype, {headers: this.userService.getAuthorizedHeader()}
    ).pipe(
      catchError(err=> throwError(err)),
      map(res=>res as SongType)
    );
  }

  setName(id: string, name: string){
    return this.http.post(this.URL+"/edit?target="+id, 
    '"'+name+'"', {headers: this.userService.getAuthorizedHeader()}
    ).pipe(
      catchError(err=> throwError(err))
    );
  }

  delete(id:string){
    return this.http.delete(this.URL+"/"+id, {headers: this.userService.getAuthorizedHeader()}
    ).pipe(
      catchError(err=> throwError(err))
    );

  }

}
