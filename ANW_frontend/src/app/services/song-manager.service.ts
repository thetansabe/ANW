import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { throwError, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';
import { Song } from '../models/song';
import { Artist } from '../models/artist';

@Injectable({
  providedIn: 'root'
})
export class SongManagerService {
  
  private URL:string =environment.HOST+"/api/song";

  constructor(private http: HttpClient, private userService: UserService) { }
  get(page: number=1, max:number=20, toggle:number=-1, keywords: string=''):Observable<SongLazyPageResponse>{
    
    if (toggle<0)
        return this.http.post(this.URL+"/page/"+max+"/"+page,'"'+keywords+'"', {headers: this.userService.getAuthorizedHeader()}).pipe(
          catchError(this.errorHandler),
          map(res=>res as SongLazyPageResponse)
        )
    else
      return this.http.post(this.URL+"/pagefilter/"+toggle+"/"+max+"/"+page,'"'+keywords+'"', {headers: this.userService.getAuthorizedHeader()}).pipe(
        catchError(this.errorHandler),
        map(res=>res as SongLazyPageResponse)
      )
  }

  upload(name:string, artists:string[],subtype:string,file: File, self: string, id: string=null)
  {
    let data=new FormData();
    data.append('name',name);
    if (artists){
      for (var i=0; i< artists.length;i++)
        data.append('artists[]',artists[i]);}
    data.append('subtype',subtype);
    if (file)
      data.append('file',file);
    data.append('self',self?"1":"0");
    if (id)
      data.append("id",id);
    return this.http.put(this.URL+"/upload",data, { headers:this.userService.getAuthorizedHeaderNoContent()})
    .pipe(
      catchError(this.errorHandler),
      map(res=>res as Song)
    );
  }

  approve(songs: string[], allow: number){
    return this.http.put(this.URL+"/approve", {
      songs: songs,
      allow: allow
    }, {headers: this.userService.getAuthorizedHeader()})
    .pipe( catchError(this.errorHandler));
  }

  delete(id: string){
    return this.http.delete(this.URL+"/delete/"+id,{ headers: this.userService.getAuthorizedHeader()})
    .pipe(catchError(this.errorHandler));
  }

  batchDelete(id: string[]){
    return this.http.put(this.URL+"/delete", id,{ headers: this.userService.getAuthorizedHeader()})
    .pipe(catchError(this.errorHandler));
  }

  errorHandler(err: HttpErrorResponse){
    return throwError(err);
  }
}
export class SongLazyPageResponse{
  list: Song[];
  artistList?: Artist[];
  totalpage?: number;
  pageResult?: number;
}