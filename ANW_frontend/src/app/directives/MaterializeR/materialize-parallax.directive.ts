import { Directive, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';

declare var M: any;
@Directive({
  selector: '[materialize-parallax]'
})
export class MaterializeParallaxDirective implements AfterViewInit, OnDestroy{

  constructor(private el: ElementRef) { }
  private instance:any;
  ngAfterViewInit(){
    M.Parallax.init(this.el.nativeElement, {
      responsiveThreshold: 3
    });
    this.instance=M.Parallax.getInstance(this.el.nativeElement);
  }
  ngOnDestroy(){
    if (this.instance)
    this.instance.destroy();
  }
}
