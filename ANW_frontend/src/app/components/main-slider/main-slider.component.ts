import { Component, OnInit, ViewChild, AfterContentInit } from '@angular/core';
import { SliderService } from '../../services/slider.service';
import { Slider } from '../../models/slider';
import { MaterializeSliderDirective } from '../../directives/MaterializeR/materialize-slider.directive';

@Component({
  selector: 'app-main-slider',
  templateUrl: './main-slider.component.html',
  styleUrls: ['./main-slider.component.css']
})
export class MainSliderComponent implements OnInit,AfterContentInit {

  sliders: Slider[];
  @ViewChild(MaterializeSliderDirective) slid: MaterializeSliderDirective;
  constructor(private slider: SliderService) { }

  ngOnInit() {
    this.slider.slideData.subscribe(val=>{
      if (val)
        this.sliders=val;
      setTimeout(()=>
      this.slid.init(), 220);
    })
    this.slider.fetch();
  }

  ngAfterContentInit(){
    this.slider.onChanged().subscribe(res=>{
      this.sliders=res;
      setTimeout(()=>this.slid.init(),500);
    })
  }

  getUrl(path: string){
    return this.slider.resource+"/"+path;
  }

}
