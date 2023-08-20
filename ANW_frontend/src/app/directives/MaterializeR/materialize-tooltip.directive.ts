import { Directive, ElementRef, AfterViewInit, Input, OnDestroy, HostListener } from '@angular/core';

declare var M:any;
@Directive({
  selector: '[materialize-tooltip]'
})
export class MaterializeTooltipDirective implements AfterViewInit, OnDestroy {

  @Input('materialize-tooltip') value:string="";
  @Input('tooltip-position') position: string="bottom";
  @Input('tooltip-margin') margin: number=5;
  private instance: any;
  private timer: any;
  constructor(private el: ElementRef) { }
  ngAfterViewInit(){
    M.Tooltip.init(this.el.nativeElement, {
      html: this.value,
      position: this.position,
      margin: this.margin
    });
    this.instance=M.Tooltip.getInstance(this.el.nativeElement);
  }

  @HostListener('mouseout')
  onmouseout(){
    if (this.instance && !this.instance.isOpen)
      return;
    if (this.timer)
      clearTimeout(this.timer);
    const self=this;
    this.timer=setTimeout(()=>{
      self.timer=null;
    },3000);
  }
  
  @HostListener('click')
  onmouseclick(){
    if (this.instance && !this.instance.isOpen)
      return;
    if (this.timer)
      clearTimeout(this.timer);
    const self=this;
    this.timer=setTimeout(()=>{
      self.instance.close();
      self.timer=null;
    },2000);
  }


  ngOnDestroy(){
    if (this.instance)
      this.instance.destroy();
  }
}
