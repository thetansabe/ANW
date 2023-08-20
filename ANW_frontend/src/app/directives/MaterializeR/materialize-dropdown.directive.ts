import { Directive, ElementRef, Input, AfterViewInit, OnDestroy } from '@angular/core';

declare var M:any;
@Directive({
  selector: '[materialize-dropdown]'
})
export class MaterializeDropdownDirective implements AfterViewInit, OnDestroy{

  @Input('materialize-dropdown') coverTrigger:boolean=false;
  @Input('dropdown-autowidth') autoWidth:boolean=true;
  @Input('dropdown-hover') hover:boolean=false;

  private instance:any;
  constructor(private el: ElementRef) {
   }

  ngAfterViewInit(){
  const elem=this.el.nativeElement;

    M.Dropdown.init(elem,{
      coverTrigger: this.coverTrigger,
      constrainWidth:!this.autoWidth,
      hover: this.hover
    })
    this.instance=M.Dropdown.getInstance(elem);  
  }
  ngOnDestroy(){
  }
}
