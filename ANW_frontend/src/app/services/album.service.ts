import { Injectable } from '@angular/core';
import { HttpClient } from '../../../node_modules/@angular/common/http';
import { UserService } from './user.service';
import { environment} from '../../environments/environment';
import { Observable, throwError } from '../../../node_modules/rxjs';
import { Album } from '../models/album';
import { catchError, map } from '../../../node_modules/rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AlbumService {

  constructor(
    private http: HttpClient,
    private userService: UserService
  ) { }

  get(id: string): Observable<Album>{
    return this.http.get(this.URL+"/"+id)
    .pipe(
      catchError(e=> throwError(e)),
      map(res=> res as Album)
    )
  }

  find(q: string, limit: number=10): Observable<Album[]>{
    return this.http.post(this.URL+"/find?q="+q+"&limit="+limit,"")
    .pipe(
      catchError(e=> throwError(e)),
      map(res=> res as Album[])
    )
  }

  private get URL(){
    return environment.HOST+"/api/album";
  }
}
