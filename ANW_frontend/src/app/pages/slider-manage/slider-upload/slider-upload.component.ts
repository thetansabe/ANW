import { Component, OnInit, AfterContentInit, ViewChildren, AfterViewInit } from '@angular/core';
import { SimpleTransition } from '../../../components/transitions/simple.transition';
import { LanguageService } from '../../../services/language.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { MaterializeDatepickerDirective } from '../../../directives/MaterializeR/materialize-datepicker.directive';
import { SliderService } from '../../../services/slider.service';
import { Slider } from '../../../models/slider';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { Router, ActivatedRoute } from '@angular/router';
import { i18n } from 'src/app/directives/MaterializeR/materialize-datepicker.i18n';

declare var M:any;
@Component({
  selector: 'app-slider-upload',
  templateUrl: './slider-upload.component.html',
  styleUrls: ['./slider-upload.component.css'],
  animations:[SimpleTransition]
})
export class SliderUploadComponent implements OnInit, AfterViewInit , AfterContentInit{

  form: FormGroup;
  file: File;
  isUploading=false;
  now= new Date();
  i18n: i18n;
  alignment=['left-align','right-align','center-align'];
  alignmentText=[this.lang.ui.leftalign,this.lang.ui.rightalign,this.lang.ui.centeralign]

  @ViewChildren(MaterializeDatepickerDirective) picker: any;
  from: MaterializeDatepickerDirective;
  to: MaterializeDatepickerDirective;
  constructor(public lang: LanguageService, private builder: FormBuilder,
    private slider: SliderService, private router: Router,
    private actRoute: ActivatedRoute
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      file:['', Validators.required],
      title:['', Validators.required],
      desc:'',
      href:'',
      align:['left-align']
    });
    this.localization();
  }

  ngAfterViewInit(){
    this.from=this.picker['first'];
    this.to=this.picker['last'];
  }
  ngAfterContentInit(){
    setTimeout(()=>
    M.updateTextFields(),300);
  }
  upload(){
    if (!this.file) return;
    this.isUploading=true;
    const title=this.form.get('title').value;
    const desc=this.form.get('desc').value;
    const href=this.form.get('href').value;
    const align=this.form.get('align').value;
    const sli: Slider={
      Id: '',
      Title:title,
      Desc:desc,
      Path:'',
      Goto: href,
      Alignment: align,
      ValidFrom: this.from.getDate(),
      ValidTo: this.to.getDate()
    };
    this.slider.upload(sli, this.file).subscribe(res=>{
      MaterializeToastService.send(this.lang.ui.message_requestsuccess);
      this.router.navigate(['../'], {relativeTo: this.actRoute });
      this.isUploading=false;
      this.slider.fetch();
    },err=>{
      this.isUploading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail, 'rounded red');
    });
  }
  setFile(event){
    if (event.target.files)
    {
      this.file=event.target.files[0];
    }
  }

  compareDate(){
    if (this.to && this.from){
      const b=this.to.getDate().valueOf()<this.from.getDate().valueOf();
      return b;
    }
    return false;
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
