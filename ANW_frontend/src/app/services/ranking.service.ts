import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { UserService } from './user.service';
import { SongRanked } from '../models/songRanked';
import { catchError, map } from 'rxjs/operators';
import { throwError, Observable } from 'rxjs';
import { Song } from '../models/song';
import { RankingCategory } from '../models/rankingCategory';
import { SongLogs } from '../models/songLogs';

@Injectable()
export class RankingService {

  constructor(
    private http: HttpClient,
    private user: UserService
  ) { }

  get(page: number=1, size: number=12){
    if (!this.user.IsAuthenticated) return new Observable<SongRankerResponse>();
    return this.http.get(this.URL+"?page="+page+"&size="+size, {headers: this.user.getAuthorizedHeader()})
    .pipe(
      catchError(e=> throwError(e)),
      map(res => res as SongRankerResponse)
    )
  }
  
  filter(type: string, page: number=1, size: number=12){
    if (!this.user.IsAuthenticated) return new Observable<SongRankerResponse>();
    return this.http.get(this.URL+"?page="+page+"&size="+size+"&type="+type, 
      {headers: this.user.getAuthorizedHeader()})
    .pipe(
      catchError(e=> throwError(e)),
      map(res => res as SongRankerResponse)
    )
  }

  summary(days: number, limit: number=5, range: number=14)
  {
    return this.http.get(this.URL+"/summary/"+days+"?limit="+limit+"&range="+range)
    .pipe(
      catchError(e=> throwError(e)),
      map(res => res as SummaryResponse[])
    )
  }

  getCategory(){
    return this.http.get(this.URL+"/category").pipe(
      catchError(e=>throwError(e)),
      map(res=>res as RankingCategory[])
    )
  }
  saveCategory(category: RankingCategory){
    if (!this.user.IsAuthenticated) return new Observable<RankingCategory>();
    if (category.Id)
      return this.http.put(this.URL+"/category", category, {headers: this.user.getAuthorizedHeader() }).pipe(
        catchError(e=>throwError(e)),
        map(res=>res as RankingCategory)
      )
    else 
      return this.http.post(this.URL+"/category", category, {headers: this.user.getAuthorizedHeader() }).pipe(
        catchError(e=>throwError(e)),
        map(res=>res as RankingCategory)
      )
  }
  deleteCategory(category: RankingCategory){
    if (!this.user.IsAuthenticated) return new Observable();
      return this.http.delete<void>(this.URL+"/category/"+category.Id, {headers: this.user.getAuthorizedHeader() }).pipe(
        catchError(e=>throwError(e))
      )
  }

  get URL(){
    return environment.HOST+"/api/ranking";
  }
}

class SongRankerResponse{
  list: SongRanked[];
  maxPage: number;
}
class SummaryResponse{
  song: Song;
  summary: SongLogs[];
}