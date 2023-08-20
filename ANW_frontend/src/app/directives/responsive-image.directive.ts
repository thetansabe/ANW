import { Directive, Input, ElementRef, AfterViewInit, AfterContentChecked } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { Subscription } from 'rxjs';

@Directive({
  selector: '[r-img]'
})
export class ResponsiveImageDirective implements AfterViewInit, AfterContentChecked {

  @Input("r-img") src: string;
  @Input("r-position") position: string;
  @Input("img-repeat") repeat: string;
  @Input("lazy-load") lazy: boolean=false;
  private isLoad: boolean=false;
  private sub: Subscription;
  constructor(
    private el:ElementRef,
    private http: HttpClient
  ) { }

  ngAfterViewInit(){
    if (this.src){
      const elem=this.el.nativeElement;
      if (!this.lazy)
        elem.style['background']="url('"+this.src+"')";
      else{
        elem.style['background']="rgba(128,128,128,0.8)";
        this.loadContent();
      }
      this.setStyle();
    }
  }

  loadContent(){
    this.sub=this.http.get(this.src, { responseType:'blob' as 'blob'}).subscribe(()=>{
      const elem=this.el.nativeElement;
      elem.style['background']="url('"+this.src+"')";
      this.isLoad=true;
      this.setStyle();
      this.unsubscribe();
    }, err =>{
      this.isLoad=false;
    })
  }

  ngAfterContentChecked(){
   if (this.src)
   {
      const elem=this.el.nativeElement;
      if (!this.lazy){
        elem.style['background']="url('"+this.src+"')";
      }
      else {
        if (!this.isLoad)
          elem.style['background']="rgba(128,128,128,0.8)";
        else{
          elem.style['background']="url('"+this.src+"')";
        }
      }
      this.setStyle();
   } 
  }

  private setStyle(){
    const elem=this.el.nativeElement;
    elem.style['background-size']="cover";
    elem.style['display']="block";
    if (this.repeat)
      elem.style['background-repeat']=this.repeat;
    else
      elem.style['background-repeat']="no-repeat";
    elem.style['minWidth']="100%";
    elem.style['minHeight']="100%";
    if (this.position)
      elem.style['background-position']=this.position;
  }
  private unsubscribe(){
    this.sub.unsubscribe();
    this.sub=null;
  }
}
