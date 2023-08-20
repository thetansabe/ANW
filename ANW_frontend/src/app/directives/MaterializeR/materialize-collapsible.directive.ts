import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';

declare var M:any;
@Directive({
  selector: '[materialize-collapsible]'
})
export class MaterializeCollapsibleDirective implements AfterViewInit, OnDestroy{

  @Input('materialize-collapsible') accordion:boolean =true;
  @Input('for-sidenav') sidenav:boolean=false;
  private instance: any;
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    this.el.nativeElement.className+=" collapsible";
    if (this.sidenav)
      this.el.nativeElement.className+=" collapsible-accordion";
    M.Collapsible.init(this.el.nativeElement, {
      accordion: this.accordion
    });
    this.instance=M.Collapsible.getInstance(this.el.nativeElement);
  }

  ngOnDestroy(){
    if (this.instance)
      this.instance.destroy();
  }
}
