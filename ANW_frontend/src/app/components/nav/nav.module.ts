import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavComponent } from './nav.component';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { RouterModule } from '@angular/router';
import { MobxAngularModule } from 'mobx-angular';
import { LookupService } from 'src/app/services/lookup.service';
import { ReactiveFormsModule } from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MaterializeRFormsModule,
    MobxAngularModule,
    RouterModule
  ],
  declarations: [
    NavComponent
  ],
  exports:[
    NavComponent
  ],
  providers: [LookupService]
})
export class NavModule { }
