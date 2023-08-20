import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MainSliderComponent } from './main-slider.component';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';

@NgModule({
  imports: [
    CommonModule,
    MaterializeMediaModule
  ],
  declarations: [
    MainSliderComponent
  ],
  exports:[
    MainSliderComponent
  ]
})
export class MainSliderModule { }
