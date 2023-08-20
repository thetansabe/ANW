import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { throwError, Subject, Observable } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { Slider } from '../models/slider';
import { UserService } from './user.service';
import { SignalrService } from './signalr.service';

@Injectable({
  providedIn: 'root'
})
export class SliderService {

  private URL:string =environment.HOST+"/api/slider";
  slideData: Subject<Slider[]>;
  resource:string=environment.HOST+"/resources/image";
  constructor(private http: HttpClient, private user: UserService,
    private hub: SignalrService
  ) { 
    this.slideData=new Subject<Slider[]>();
  }


  fetch(){
    return this.http.get(this.URL)
    .pipe(
      catchError(this.errorHandler),
      map(res=> res as Slider[])
    ).subscribe(val=>{
      this.slideData.next(val);
    });
  }

  get(){
    return this.http.get(this.URL+"/manage",{headers: this.user.getAuthorizedHeader()})
    .pipe(
      catchError(this.errorHandler),
      map(res=> res as Slider[])
    )
  }
  
  getById(id: string){
    return this.http.get(this.URL+"/"+id,{headers: this.user.getAuthorizedHeader()})
    .pipe(
      catchError(this.errorHandler),
      map(res=> res as Slider)
    )
  }

  upload(slider: Slider, file: File){
    let form=new FormData();
    form.append("title",slider.Title);
    form.append("desc",slider.Desc);
    form.append("alignment",slider.Alignment);
    form.append("validFrom",slider.ValidFrom.toUTCString());
    form.append("validTo",slider.ValidTo.toUTCString());
    form.append("file",file);
    return this.http.put(this.URL+"/upload",form,{headers: this.user.getAuthorizedHeaderNoContent()})
    .pipe(catchError(this.errorHandler));
  }

  delete(id: string){
    return this.http.delete(this.URL+"/delete/"+id,{headers: this.user.getAuthorizedHeader()})
    .pipe(catchError(this.errorHandler));
  }

  edit(slider: Slider){
    return this.http.put(this.URL+"/edit", slider, {headers: this.user.getAuthorizedHeader()})
    .pipe( catchError(this.errorHandler));
  }

  onChanged(): Observable<Slider[]>{
    return this.hub.on('onSliderChanged').pipe(
      map(res=> JSON.parse(res+'') as Slider[])
    );
  }
  
  errorHandler(err: HttpErrorResponse){
    return throwError(err);
  }
}
