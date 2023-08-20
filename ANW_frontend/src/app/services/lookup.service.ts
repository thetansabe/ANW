import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {environment} from '../../environments/environment';
import { throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Song } from '../models/song';
import { Artist } from '../models/artist';
import { Playlist } from '../models/playlist';
import { Album } from '../models/album';

@Injectable()
export class LookupService {

  constructor(
    private http: HttpClient
  ) { }
  
  findMusic(keyword: string, page: number=1, size: number=6){
    return this.http.post(this.URL+"?keyword="+keyword+"&page="+page+"&size="+size,"")
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as MusicLookupResponse)
    )
  }
  findArtist(keyword: string){
    return this.http.post(this.URL+"/more?keyword="+keyword,"")
    .pipe(
      catchError(e=>throwError(e)),
      map(res=>res as MusicLookupResponse)
    )
  }


  private get URL(){
    return environment.HOST+"/api/lookup";
  }
}
class MusicLookupResponse{
  songList?: Song[];
  artistList?: Artist[];
  playlist?: Playlist[];
  albumList?: Album[];
  maxPage?: number;
}

