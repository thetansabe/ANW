import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterializeParallaxDirective } from '../materialize-parallax.directive';
import { MaterializeSidenavDirective } from '../materialize-sidenav.directive';
import { MaterializeFabDirective } from '../materialize-fab.directive';
import { MaterializeSliderDirective } from '../materialize-slider.directive';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations:[
    MaterializeParallaxDirective,
    MaterializeSidenavDirective,
    MaterializeFabDirective,
    MaterializeSliderDirective
  ],
  exports:[
    MaterializeParallaxDirective,
    MaterializeSidenavDirective,
    MaterializeFabDirective,
    MaterializeSliderDirective
  ]
})
export class MaterializeMediaModule { }
