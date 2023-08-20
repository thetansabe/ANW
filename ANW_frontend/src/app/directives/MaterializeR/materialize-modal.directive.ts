import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';

declare var M:any;  
@Directive({
  selector: '[materialize-modal]'
})
export class MaterializeModalDirective implements AfterViewInit, OnDestroy {

  @Input('materialize-modal') dismissable:boolean=true;
  constructor(private el: ElementRef) { }

  private instance: any;
  ngAfterViewInit(){
    M.Modal.init(this.el.nativeElement,{
      dismissible: this.dismissable,
      preventScrolling: false
    });
    this.instance=M.Modal.getInstance(this.el.nativeElement);
  }

  open(){
    this.instance.open();
  }
  close(){
    this.instance.close();
  }
  destroy(){
    this.instance.destroy();
  }
  
  ngOnDestroy(){
  }
}
