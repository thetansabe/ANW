import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';

declare var M:any;
@Directive({
  selector: '[materialize-select]'
})
export class MaterializeSelectDirective implements AfterViewInit, OnDestroy {

  @Input('materialize-select') value: string;
  private instance:any;
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    if (this.value)
      this.el.nativeElement.value=this.value;
    M.FormSelect.init(this.el.nativeElement, {}); 
    this.instance=M.FormSelect.getInstance(this.el.nativeElement);
  }
  ngOnDestroy(){
    if (this.instance)
      this.instance.destroy();
  }
}
