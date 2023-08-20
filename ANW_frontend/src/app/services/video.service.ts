import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { environment } from 'src/environments/environment';
import { catchError, map } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { Video } from '../models/video';

@Injectable({
  providedIn: 'root'
})
export class VideoService {

  constructor(private http: HttpClient, private user: UserService) { }

  upload(name: string, artists: string[], type: string, file: File)
  {

    let data=new FormData();
    data.append('name',name);
    if (artists){
      for (var i=0; i< artists.length;i++)
        data.append('artists[]',artists[i]);}
    data.append('type',type);
    if (file)
      data.append('file',file);
    return this.http.post(this.URL+"/upload",data, { headers:this.user.getAuthorizedHeaderNoContent()})
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as Video)
    );
  }

  get(size: number =12){
    return this.http.get(this.URL+"?size="+size).pipe(
      catchError(e=>throwError(e)),
      map(res=>res as VideoPackageResponse)
    )
  }

  getById(id: string){
    return this.http.post(this.URL+"/"+id,"").pipe(
      catchError(e=>throwError(e)),
      map(res=>res as Video)
    )
  }

  filter(page: number, size: number =12, q: string=""){
    if (!this.user.IsAuthenticated) return null;
    return this.http.get(this.URL+"/manage?page="+page+"&size="+size+"&q="+q, {headers: this.user.getAuthorizedHeader() }).pipe(
      catchError(e=>throwError(e)),
      map(res=>res as VideoPackageResponse)
    )
  }

  approve(id: string[], approve: number)
  {
    if (!this.user.IsAuthenticated) return null;
    return this.http.put(this.URL+"/approve", {id: id, approve: approve }, {headers: this.user.getAuthorizedHeader() }).pipe(
      catchError(e=>throwError(e)),
      map(res=>res as Video[])
    )
  }

  private get URL(){
    return environment.HOST+"/api/video";
  }
}
class VideoPackageResponse{
  maxPage: number;
  list: Video[];
}