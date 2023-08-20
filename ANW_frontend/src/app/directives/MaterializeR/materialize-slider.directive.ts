import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';
declare var M:any;
@Directive({
  selector: '[materialize-slider]'
})
export class MaterializeSliderDirective implements AfterViewInit, OnDestroy{

  @Input('slider-height') height: number= 400;
  constructor(private el: ElementRef) { }
  private instance: any;

  ngAfterViewInit(){
    var elems = this.el.nativeElement;
    M.Slider.init(elems, { height: this.height});
    this.instance=M.Slider.getInstance(elems);
  }

  init(){
    this.destroy();
    var elems = this.el.nativeElement;
    M.Slider.init(elems, { height: this.height});
    this.instance=M.Slider.getInstance(elems);
  }

  destroy(){
    if (this.instance)
      this.instance.destroy();
  }

  ngOnDestroy(){
    this.destroy();
  }

}
