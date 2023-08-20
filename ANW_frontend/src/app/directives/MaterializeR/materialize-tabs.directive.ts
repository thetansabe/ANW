import { Directive, AfterViewInit, ElementRef, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

declare var M:any;
@Directive({
  selector: '[materialize-tabs]'
})
export class MaterializeTabsDirective implements AfterViewInit {

  @Input('materialize-tabs') swipeable:boolean=false;
  private instance:any;
  private currentPage: Subject<number>;
  private tabElement: Element[];
  private container: Element;
  constructor(private el: ElementRef) { }
  ngAfterViewInit(){
    const ele=this.el.nativeElement;
    this.currentPage=new Subject<number>();
    const self=this;
    M.Tabs.init(ele, {
      swipeable: this.swipeable,
      responsiveThreshold: 6000,
      onShow:()=>{
        if (self.instance){
          self.currentPage.next(self.instance.index);
        }
      }
    });
    this.instance=M.Tabs.getInstance(ele);
    self.initElement();
    if (this.swipeable==false) return;
      this.pageSubcribe();
  }


  update(){
    if (this.instance){
      this.currentPage.next(this.instance.index);
    }
  }

  pageSubcribe(){
    this.currentPage.pipe(
      debounceTime(400)
    ).subscribe(val=>{
      const height=this.tabElement[val].clientHeight;
      this.container['style'].height=height+"px";
    })
  }

  initElement(){
    if (this.swipeable==false) return;
    const ele=this.el.nativeElement;
    const parent:Element=this.el.nativeElement['parentNode'];
    this.container=parent.querySelectorAll(".tabs-content")[0];
    this.tabElement=[];
    console.log(parent);
    if (!this.container) return;
    const arr=this.container.querySelectorAll('.carousel-item');
    for (var i=0; i<arr.length;i++){
      this.tabElement.push(arr[i]);
      arr[i]['style'].height='auto';
    }
    this.container['style'].transition="all 0.3s ease-in-out";
  }
}
