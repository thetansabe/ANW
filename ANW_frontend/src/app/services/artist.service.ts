import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { environment } from '../../environments/environment';
import { catchError, map } from 'rxjs/operators';
import { throwError, Observable } from 'rxjs';
import { Artist } from '../models/artist';

@Injectable({
  providedIn: 'root'
})
export class ArtistService {

  private URL: string= environment.HOST+"/api/artist";
  constructor(
    private http: HttpClient,
    private userService: UserService
  ) { }

  get(page: number=1, size: number=12){
    return this.http.get(this.URL+"?page="+page+"&size="+size)
    .pipe(
      catchError((err)=>throwError(err)),
      map(res=>res as ArtistListResponse)
    );
  }

  
  getToManage(page: number=1, size: number=12, q: string=null){
    let path="page="+page+"&size="+size;
    if (q)
      path+="&q="+q;
    return this.http.get(this.URL+"/manage?"+path, {headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError((err)=>throwError(err)),
      map(res=>res as ArtistListResponse)
    );
  }


  getById(id: string){
    return this.http.post(this.URL+"/get/"+id,"")
    .pipe(
      catchError((err)=>throwError(err)),
      map(res=>res as Artist)
    );
  }

  find(keyword: string="", page: number=1, size: number=12){
    if (!keyword)
      return this.get(page,size);
    return this.http.post(this.URL+"?page="+page+"&size="+size+"&q="+keyword,"")
    .pipe(
      catchError((err)=>throwError(err)),
      map(res=>res as ArtistListResponse)
    );
  }

  startWith(alphabet: string="", page: number=1, size:number=12){
    if (!alphabet)
      return this.get(page,size);
    return this.http.get(this.URL+"?q="+alphabet+"&page="+page+"&size="+size)
    .pipe(
      catchError(err=>throwError(err)),
      map(res=>res as ArtistListResponse)
    )
  }

  save(artist: Artist): Observable<Artist>{
    if (!artist) return null;
    return this.http.put(this.URL+"/"+artist.Id,artist,{
      headers: this.userService.getAuthorizedHeader()
    }).pipe(
      catchError(err=>throwError(err)),
      map(res=>res as Artist)
    )
  }

  delete(artist: Artist): Observable<any>{
    if (!artist) return null;
    return this.http.delete(this.URL+"/"+artist.Id, {headers: this.userService.getAuthorizedHeader()})
    .pipe(
      catchError(err=>throwError(err))
    )
  }

  batchDelete(artistList: Artist[]): Observable<any>{
    if (!artistList) return null;
    const Ids: string[]=[];
    for (var i=0; i<artistList.length; i++)
      Ids.push(artistList[i].Id);
      return this.http.post(this.URL+"/delete", Ids,{headers: this.userService.getAuthorizedHeader()})
      .pipe(
        catchError(err=>throwError(err)),
        map(res=>res as Artist[])
      )
  }

  uploadAvatar(id: string, file: File){
    const form=new FormData();
    form.append("id",id);
    form.append("file",file);
    return this.http.post(this.URL+"/upload/avatar",form, {headers: this.userService.getAuthorizedHeaderNoContent(), 
        responseType: "text"})
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as string)
    )
  }
  uploadBackground(id: string, file: File){
    const form=new FormData();
    form.append("id",id);
    form.append("file",file);
    return this.http.post(this.URL+"/upload/background",form, {headers: this.userService.getAuthorizedHeaderNoContent(),
    responseType: "text"})
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as string)
    )
  }
}

class ArtistListResponse{
  page?: number;
  maxPage?: number;
  list?: Artist[];
}