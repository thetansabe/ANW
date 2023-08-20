import { Directive, ElementRef, AfterViewInit, Input, OnDestroy, HostListener } from '@angular/core';
import { Observable, of } from 'rxjs';

declare var M:any;
@Directive({
  selector: '[materialize-chips]'
})

export class MaterializeChipsDirective implements AfterViewInit, OnDestroy{

  @Input('materialize-chips') placeholder:string='';
  @Input('chips-placeholder') secondHolder:string='';
  @Input('chips-dictionary') dict: any;
  @Input('max-chips') max: number=99;
  private instance:any;
  constructor(private el: ElementRef) { }

  ngAfterViewInit(){
    if (!this.dict)
    {
        M.Chips.init(this.el.nativeElement, {
        placeholder: this.placeholder,
        secondaryPlaceholder:this.secondHolder,
        limit: this.max
      });
    }
    else {
      let opt=new chipDictionaryOption();
      opt.data=this.dict;
      M.Chips.init(this.el.nativeElement, {
        placeholder: this.placeholder,
        secondaryPlaceholder:this.secondHolder,
        autocompleteOptions: opt,
        limit: this.max
      });
    }
    this.instance=M.Chips.getInstance(this.el.nativeElement);
    const ele:Element=this.el.nativeElement.querySelectorAll("input.input")[0];
    const self=this;
    ele.addEventListener('focusout',()=>{
      self.update();
    })
  }
  add(obj: materializeChip){
    this.instance.addChip(obj);
  }
  addArr(obj: materializeChip[]){
    for (var i=0; i<obj.length;i++)
      this.instance.addChip(obj[i]);
  }
  clear(){
    for (var i=this.instance.chipsData.length-1; i>=0; i--)
      this.delete(i);
  }
  update(){
    const ele=this.el.nativeElement.querySelectorAll("input.input")[0];
    if (!ele.value) return;
    const value=(ele.value+'').trim();
    if (value.length>0)
    {
      
      const chip:materializeChip={
        tag: ele.value
      };
      ele.value="";
      this.add(chip);
    }
  }
  delete(index){
    this.instance.deleteChip(index);
  }
  values(){
    return this.instance.chipsData;
  }

  get tags(){
    return this.instance.chipsData as materializeChip[];
  }

  ngOnDestroy(){
    if (this.instance)
      this.instance.destroy();
  }

}

export class materializeChip{
  tag: string;
  image?: string;
}
export class chipDictionaryOption{
  data: any;
  limit: number=99;
  minLength:number=1;
}