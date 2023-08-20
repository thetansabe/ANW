import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpErrorResponse, HttpResponse, HttpHeaders } from '@angular/common/http';
import { throwError, Subject, Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Song } from '../models/song';
import { UserService } from './user.service';
import { SongType } from '../models/songtype';

@Injectable({
  providedIn: 'root'
})
export class SongService {
  private URL:string =environment.HOST+"/api/song";
  constructor(private http: HttpClient, private userService: UserService) { }

  get(id: string):Observable<Song>{
    return this.http.get(this.URL+"/detail/"+id).pipe(
      catchError(this.errorHandler),
      map(res=> res as Song)
    )
  }

  fetch(){
    return this.http.get(this.URL,{}).pipe(
      catchError(this.errorHandler),
      map(res=> res as Song[])
    );
  }

  find(q: string, limit: number=5){
    return this.http.post(this.URL+"/find?q="+q+"&limit="+limit,"").pipe(
      catchError(this.errorHandler),
      map(res=> res as Song[])
    );
  }

  download(song: Song): Observable<{}>{
    const result=new Subject();
    this.http.post(environment.HOST+"/api/song/download/" + song.Id,"",{ responseType: 'arraybuffer'})
    .subscribe(val=> {
      if (val) {
          const a: HTMLElement = document.createElement("a");
          var url = window.URL.createObjectURL(new Blob([val], { type: "application/octet-stream" }));
          a.setAttribute("href",url);
          a.setAttribute("download", song.Name + "." + song.Paths[0]['Extension']);
          a.click();
          window.URL.revokeObjectURL(url);
          a.remove();
          result.next(song.Name);
      }
    }, err=> result.error(err));
    return result.asObservable();
  }

  toggleFavorite(song: Song){
    return this.http.put(this.URL+"/favorite/"+song.Id,"",{headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(this.errorHandler),
      map(res=>res as string[])
    )
  }

  getFavorite(){
    return this.http.get(this.URL+"/favorite",{headers:this.userService.getAuthorizedHeader()}).pipe(
      catchError(this.errorHandler),
      map(res=> res as Song[])
    );
  }

  errorHandler(err: HttpErrorResponse){
    return throwError(err);
  }
}
