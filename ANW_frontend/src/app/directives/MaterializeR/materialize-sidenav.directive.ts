import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';

declare var M: any;
@Directive({
  selector: '[materialize-sidenav]'
})
export class MaterializeSidenavDirective implements AfterViewInit, OnDestroy {

  @Input('materialize-sidenav') edge:string='left';
  @Input('sidenav-scroll') scrollable:boolean=false;
  instance:any;
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    if (!this.edge) this.edge='left';
      M.Sidenav.init(this.el.nativeElement, {
        edge:this.edge,
        preventScrolling: !this.scrollable
      });
      this.instance=M.Sidenav.getInstance(this.el.nativeElement);
  }

  open(){
    this.instance.open();
  }
  close(){
    this.instance.close();
  }
  ngOnDestroy(){
    this.instance.destroy();
  }
}
