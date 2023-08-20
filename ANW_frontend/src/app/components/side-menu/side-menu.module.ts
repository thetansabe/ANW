import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SideMenuComponent } from './side-menu.component';
import { RouterModule } from '@angular/router';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';

@NgModule({
  imports: [
    CommonModule,
    RouterModule,
    MaterializeRFormsModule
  ],
  declarations: [
    SideMenuComponent
  ],
  exports:[
    SideMenuComponent
  ]
})
export class SideMenuModule { }
