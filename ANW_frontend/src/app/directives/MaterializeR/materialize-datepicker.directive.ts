import { Directive, ElementRef, AfterViewInit, Input, OnDestroy } from '@angular/core';
import { i18n } from './materialize-datepicker.i18n';

declare var M: any;
@Directive({
  selector: '[materialize-datepicker]'
})
export class MaterializeDatepickerDirective implements AfterViewInit, OnDestroy {

  @Input('datepicker-format') format: string;
  @Input('datepicker-default') defaultDate: Date;
  @Input('datepicker-localization') language: i18n;

  constructor(private el: ElementRef) { }

  private instance:any;
  ngAfterViewInit(){
    const ele=this.el.nativeElement;
    if (!this.language) this.language=new i18n();
    if (this.language){
      M.Datepicker.init(ele, {
        format: this.format,
        defaultDate: this.defaultDate,
        setDefaultDate: this.defaultDate!=null,
        i18n: this.language
      });
    }
    else{
      M.Datepicker.init(ele, {
        format: this.format,
        defaultDate: this.defaultDate,
        setDefaultDate: this.defaultDate!=null,
      });
    }
    this.instance=M.Datepicker.getInstance(ele);
  }

  setValue(value: string){
    this.el.nativeElement.value=value;
  }

  getDate():Date{
    return this.instance.date as Date;
  }

  setDate(time: Date){
    this.instance.setDate(time);
    this.instance.gotoDate(time);
  }

  ngOnDestroy(){
    if (this.instance)
      this.instance.destroy();
  }
}
