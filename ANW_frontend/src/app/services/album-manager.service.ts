import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '../../../node_modules/@angular/common/http';
import { UserService } from './user.service';
import { catchError, map } from '../../../node_modules/rxjs/operators';
import { throwError, Observable } from '../../../node_modules/rxjs';
import { Album } from '../models/album';
import { Artist } from '../models/artist';

@Injectable()
export class AlbumManagerService {

  constructor(
    private http: HttpClient,
    private userService: UserService
  ) { }
  
  get(page: number=1, size: number=12, q: string=null): Observable<AlbumListResponse>{
    let path=this.URL+"/list?page="+page+"&limit="+size;
    if (q!=null)
      path+="&q="+q;
    return this.http.post(path,"", { headers: this.userService.getAuthorizedHeader()})
    .pipe( 
      catchError(e=>throwError(e)),
      map(res => res as AlbumListResponse)
    );
  }

  create(album: Album, artists: string[], cover: File){
    const data=new FormData();
    data.append("name",album.Name);
    data.append("desc",album.Desc);
    if (artists)
      artists.forEach(a=> data.append("artists[]",a));
    if (album.SongList)
      album.SongList.forEach(s=> data.append("songlist[]",s.Id));
    data.append("file",cover);
    return this.http.post(this.URL,data, {headers: this.userService.getAuthorizedHeaderNoContent() })
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as Album)
    )
  }

  save(album: Album, artist: string[]): Observable<Album>{
    const artistList: string[]=[];
    const songList: string[]=[];
    const form= new FormData();
    if (artist)
      artist.forEach(a=> form.append("artists[]", a));
    if (album.SongList)
      album.SongList.forEach(s=> form.append("songlist[]", s.Id));
    form.append("id", album.Id);
    form.append("name", album.Name);
    form.append("desc", album.Desc);
      
    return this.http.put(this.URL, form, {headers: this.userService.getAuthorizedHeaderNoContent() })
      .pipe(
        catchError(e=> throwError(e)),
        map( res=> res as Album)
      );
  }
  
  delete(album: Album): Observable<void>{
    return this.http.delete<void>(this.URL+"/"+album.Id , {headers: this.userService.getAuthorizedHeader() })
    .pipe(
      catchError(e=> throwError(e))
    );
  }

  batchDelete(list: Album[]){
    const arr: string[]=[];
    list.forEach(alb=> arr.push(alb.Id));
    return this.http.post(this.URL+"/delete", arr, {headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(e=>throwError(e)),
      map(res => res as string[])
    )
  }

  changeImage(album: Album, file: File): Observable<Album>{
    const data: FormData=new FormData();
    data.append("id", album.Id);
    if (file)
      data.append("file", file);
    return this.http.post(this.URL+"/upload",data,{headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(e=> throwError(e)),
      map(res => res as Album)
    );
  }

  private get URL(): string{
    return environment.HOST+"/api/album";
  }

}
class AlbumListResponse{
  list: Album[];
  maxPage: number;
};