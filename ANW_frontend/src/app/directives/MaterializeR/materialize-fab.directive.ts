import { Directive, ElementRef, AfterViewInit, Input } from '@angular/core';

declare var M:any;
@Directive({
  selector: '[materialize-fab]'
})
export class MaterializeFabDirective implements AfterViewInit {

  @Input('materialize-fab') direction: string='top';
  @Input('fab-hover') hover:boolean=true;
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    M.FloatingActionButton.init(this.el.nativeElement, {
      direction: this.direction,
      hoverEnabled: this.hover
    });
  }
}
