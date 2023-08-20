import { Injectable } from '@angular/core';
import { Song } from '../models/song';
import { Playlist, PlaylistPageResponse, PlaylistResponse } from '../models/playlist';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';
import { throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PlaylistService {
  private URL:string =environment.HOST+"/api/playlist";

  constructor(private http: HttpClient, private user: UserService) { }

  create(list: Playlist, file: File=null){
    const data=new FormData();
    data.append("model",JSON.stringify(list));
    if (file)
      data.append("file",file);
      
    return this.http.post(this.URL+"/save",data,{headers: this.user.getAuthorizedHeaderNoContent()})
    .pipe(
      catchError((err)=>{
        return throwError(err);
      }),
      map(res=>res as Playlist)
    )
  }
  save(list: Playlist){
    return this.http.put(this.URL+"/save", "'"+JSON.stringify(list)+"'", {headers:this.user.getAuthorizedHeader()})
    .pipe(
      catchError((err)=>{
        return throwError(err);
      }),
      map(res=>res as Playlist)
    )
  }
  getpage(page: number, limit: number){
    return this.http.get(this.URL+"/getall?page="+page+"&limit="+limit,{headers:this.user.getAuthorizedHeader()})
    .pipe(
      catchError((err)=>{
        return throwError(err);
      }),
      map(res=> res as PlaylistPageResponse)
    )
  }
  getmy(limit: number){
    return this.http.get(this.URL+"/getmy/"+limit,{headers:this.user.getAuthorizedHeader()})
    .pipe(
      catchError((err)=>{
        return throwError(err);
      }),
      map(res=> res as Playlist[])
    )
  }
  find(keyword: string){
    return this.http.post(this.URL+"/find", '"'+keyword+'"')
    .pipe(catchError((err)=>
    {
      return throwError(err);
    }),
    map(res=>res as Playlist[])
    );
  }
  findmy(keyword: string){
    return this.http.post(this.URL+"/findmy", '"'+keyword+'"', {headers:this.user.getAuthorizedHeader()})
    .pipe(catchError((err)=>
    {
      return throwError(err);
    }),
    map(res=>res as Playlist[])
    );
  }
  get(playlistId: string){
    if (this.user.IsAuthenticated)
      return this.http.get(this.URL+"/get/"+playlistId, {headers: this.user.getAuthorizedHeader()})
      .pipe(
        catchError((err)=>{
          return throwError(err);
        }),
        map(res=> res as PlaylistResponse)
      );
    return this.http.get(this.URL+"/get/"+playlistId)
    .pipe(
      catchError((err)=>{
        return throwError(err);
      }),
      map(res=> res as PlaylistResponse)
    )
  }
  delete(playlistId: string){
    return this.http.delete(this.URL+"/delete/"+playlistId, {headers: this.user.getAuthorizedHeader()})
    .pipe(
      catchError((err)=>{
        return throwError(err);
      })
    )
  }
}
