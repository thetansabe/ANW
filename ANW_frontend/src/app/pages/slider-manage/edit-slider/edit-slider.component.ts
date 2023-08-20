import { Component, OnInit, OnDestroy, ViewChildren, AfterViewInit, ViewChild } from '@angular/core';
import { SliderService } from '../../../services/slider.service';
import { ActivatedRoute, Router } from '../../../../../node_modules/@angular/router';
import { Slider } from '../../../models/slider';
import { Subscription } from '../../../../../node_modules/rxjs';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { LanguageService } from '../../../services/language.service';

import { MaterializeDatepickerDirective } from '../../../directives/MaterializeR/materialize-datepicker.directive';
import { LoaderService } from '../../../services/loader.service';
import { i18n } from 'src/app/directives/MaterializeR/materialize-datepicker.i18n';

@Component({
  selector: 'app-edit-slider',
  templateUrl: './edit-slider.component.html',
  styleUrls: ['./edit-slider.component.css']
})
export class EditSliderComponent implements OnInit, AfterViewInit, OnDestroy {

  currentSlide: Slider;
  isLoading=false;
  i18n: i18n;
  alignment=['left-align','right-align','center-align'];
  alignmentText=[this.lang.ui.leftalign,this.lang.ui.rightalign,this.lang.ui.centeralign]

  now: Date=new Date();

  @ViewChildren(MaterializeDatepickerDirective) picker: any;
  from: MaterializeDatepickerDirective;
  to: MaterializeDatepickerDirective;

  constructor(
    private sliderService: SliderService,
    private actRoute: ActivatedRoute,
    private router: Router,
    public lang: LanguageService,
    private loader: LoaderService
  ) { }

  private subscriber: Subscription;
  ngOnInit() {
    this.localization();
    this.subscriber=this.actRoute.queryParams.subscribe(params=>{
      if (params['id'])
      {
        this.get(params['id']);
      }
      else this.router.navigate(['/notfound']);
    });
  }

  
  ngAfterViewInit(){
    this.from=this.picker['first'];
    this.to=this.picker['last'];
    console.log(this.from);
    if (this.currentSlide){
      this.from.setDate(new Date(this.currentSlide.ValidFrom));
      this.to.setDate(new Date(this.currentSlide.ValidTo));
      // console.log(this.from.getDate());
      this.from.setValue((this.from.getDate().getDate())+"/"+
        (1+this.from.getDate().getMonth())+"/"+
        this.from.getDate().getFullYear());
      this.to.setValue((this.to.getDate().getDate())+"/"+
          (1+this.to.getDate().getMonth())+"/"+
          this.to.getDate().getFullYear());
    }
  }

  get(id: string){
    this.loader.isSubLoading=true;
    this.sliderService.getById(id).subscribe(slide=>{
      this.loader.isSubLoading=false;
      this.currentSlide=slide;
      this.ngAfterViewInit();
      if (this.from && this.to)
      {
        this.from.setDate(new Date(slide.ValidFrom));
        this.to.setDate(new Date(slide.ValidTo));
        this.from.setValue((this.from.getDate().getDate())+"/"+
          (1+this.from.getDate().getMonth())+"/"+
          this.from.getDate().getFullYear());
        this.to.setValue((this.to.getDate().getDate())+"/"+
            (1+this.to.getDate().getMonth())+"/"+
            this.to.getDate().getFullYear());
      }
    },err=>{
      this.router.navigate(['/notfound']);
    });
  }

  save(){
    if (!this.compareDate()) return;
    this.isLoading=true;
    this.currentSlide.ValidFrom=this.from.getDate();
    this.currentSlide.ValidTo=this.to.getDate();
    this.sliderService.edit(this.currentSlide).subscribe(res=>{
      MaterializeToastService.send(this.lang.ui.message_saved,"rounded");
      this.isLoading=false;
      this.router.navigate(['..']);
    }, err=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,"rounded red");
    })
  }

  compareDate(){
    if (!this.from || !this.to) return false;
    if (this.from.getDate()>=this.to.getDate())
      return false;
    if (this.to.getDate()<new Date())
      return false;
    return true;
  }

  getUrl(path: string){
    return this.sliderService.resource+"/"+path;
  }

  ngOnDestroy(){
    this.subscriber.unsubscribe();
  }
  
  localization(){
    this.i18n=new i18n();
    this.i18n.cancel=this.lang.ui.cancel;
    this.i18n.clear=this.lang.ui.clear;
    this.i18n.done=this.lang.ui.ok;
    this.i18n.months=[
      this.lang.ui.feb,
      this.lang.ui.feb,
      this.lang.ui.mar,
      this.lang.ui.apr,
      this.lang.ui.may,
      this.lang.ui.jun,
      this.lang.ui.jul,
      this.lang.ui.aug,
      this.lang.ui.sep,
      this.lang.ui.oct,
      this.lang.ui.nov,
      this.lang.ui.dec
    ];
    this.i18n.monthsShort=[
      this.lang.ui.s_feb,
      this.lang.ui.s_feb,
      this.lang.ui.s_mar,
      this.lang.ui.s_apr,
      this.lang.ui.s_may,
      this.lang.ui.s_jun,
      this.lang.ui.s_jul,
      this.lang.ui.s_aug,
      this.lang.ui.s_sep,
      this.lang.ui.s_oct,
      this.lang.ui.s_nov,
      this.lang.ui.s_dec
    ];
    this.i18n.weekdays=[
      this.lang.ui.sunday,
      this.lang.ui.monday,
      this.lang.ui.tuesday,
      this.lang.ui.wednesday,
      this.lang.ui.thursday,
      this.lang.ui.friday,
      this.lang.ui.saturday
    ]
    this.i18n.weekdaysShort=[
      this.lang.ui.s_sunday,
      this.lang.ui.s_monday,
      this.lang.ui.s_tuesday,
      this.lang.ui.s_wednesday,
      this.lang.ui.s_thursday,
      this.lang.ui.s_friday,
      this.lang.ui.s_saturday
    ]
    this.i18n.weekdaysAbbrev=[
      this.lang.ui.a_sunday,
      this.lang.ui.a_monday,
      this.lang.ui.a_tuesday,
      this.lang.ui.a_wednesday,
      this.lang.ui.a_thursday,
      this.lang.ui.a_friday,
      this.lang.ui.a_saturday
    ]
  }
}
