import { NgModule } from '@angular/core';
import { ResponsiveImageDirective } from './responsive-image.directive';

@NgModule({
  declarations: [
    ResponsiveImageDirective
  ],
  exports:[
    ResponsiveImageDirective
  ]
})
export class ResponsiveImageModule { }
