import { Component, OnInit, AfterContentInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup, FormBuilder } from '@angular/forms';
import { SliderService } from '../../services/slider.service';
import { Slider } from '../../models/slider';
import { Router, ActivatedRoute } from '@angular/router';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { LanguageService } from '../../services/language.service';
import { LoaderService } from '../../services/loader.service';
import { MaterializeModalDirective } from '../../directives/MaterializeR/materialize-modal.directive';
import { observable } from 'mobx-angular';
import { SimpleTransition } from '../../components/transitions/simple.transition';

@Component({
  selector: 'app-slider-manage',
  templateUrl: './slider-manage.component.html',
  styleUrls: ['./slider-manage.component.css'],
  animations:[
    SimpleTransition
  ]
})
export class SliderManageComponent implements OnInit, AfterContentInit {

  form: FormGroup;
   slideList: Slider[];
   slideOffset: Slider[]=[];
   selectedSlider: Slider;
   isLoading=false;
   now: Date;
   converted={};
   maxSize: number=6;
   pages: number[];
   maxPage: number=1;
   page: number=1;
  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  constructor(private builder: FormBuilder, private slider: SliderService,
    private router: Router,public lang: LanguageService,
    private loader: LoaderService,
    private actRoute: ActivatedRoute
  ) { }

  ngOnInit() {
    this.loader.isSubLoading=true;
    this.actRoute.queryParams.subscribe(val=>{
      if (val['page']){
        this.page=+val['page'];
        if (this.slideList)
          this.setPage(this.page);
      }
    })
  }

  ngAfterContentInit(){
    this.fetchAll();
    this.slider.onChanged().subscribe(res=>{
      this.slideList=res;
      this.calcPage();
      this.now=new Date();
      res.forEach(slide=>this.converted[slide.Id]=new Date(slide.ValidTo));
    });
  }

  fetchAll(){
    this.now=new Date();
    this.slider.get().subscribe(val=>{
      this.slideList=val;
      this.loader.isSubLoading=false;
      this.calcPage();
      val.forEach(slide=>this.converted[slide.Id]=new Date(slide.ValidTo));
    },err=>{
      if (err.status==401 || err.status==403)
        this.router.navigate(['/']);
      else{
        MaterializeToastService.send(this.lang.ui.message_requestfail,'rounded red');
      }
    });
  }

  isExpired(slider: Slider){
    return this.converted[slider.Id].getTime() <this.now.getTime();
  }

  calcPage(){
    this.pages=[];
    this.maxPage=Math.ceil(this.slideList.length/this.maxSize);
    for (var i=1; i<this.maxPage;i++)
      this.pages.push(i);
    if (this.page>this.maxPage)
      this.page=this.maxPage;
    this.setPage(this.page);
  }

  setPage(page: number){
    if (page<=0 || page>this.maxPage) return;
    this.page=page;
    const offset=(page-1)*this.maxSize;
    const maxOffset=(offset+this.maxSize)>this.slideList.length?this.slideList.length:offset+this.maxSize;
    this.slideOffset=this.slideList.slice(offset,maxOffset);
  }

  delete(){
    if (this.selectedSlider)
    {
      this.isLoading=true;
      this.slider.delete(this.selectedSlider.Id)
      .subscribe(res=>{
        this.isLoading=false;
        this.modal.close();
        MaterializeToastService.send(this.lang.ui.message_deletedone,"rounded green");

      },err=>{
        this.isLoading=false;
        MaterializeToastService.send(this.lang.ui.message_deleteunable,"rounded red");
      })
    }
  }

  setSlider(slider: Slider){
    this.selectedSlider=slider;
  }

  getUrl(path: string){
    return this.slider.resource+"/"+path;
  }
}
