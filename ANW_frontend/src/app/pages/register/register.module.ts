import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { RegisterComponent } from './register.component';
import { MaterializeParallaxDirective } from '../../directives/MaterializeR/materialize-parallax.directive';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';
import { MobxAngularModule } from 'mobx-angular';



const route=[
    { path:'', component: RegisterComponent }
]
@NgModule({
  declarations: [
      RegisterComponent
  ],
  imports: [
      CommonModule,
      FormsModule,
      ReactiveFormsModule,
      MaterializeMediaModule,
      MaterializeRFormsModule,
      MobxAngularModule,
      RouterModule.forChild(route)
  ],
})
export class MyRegisterModule { }
