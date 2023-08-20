import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartJsDirective } from './chart-js.directive';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [
    ChartJsDirective
  ],
  exports:[
    ChartJsDirective
  ]
})
export class ChartJsModule { }
