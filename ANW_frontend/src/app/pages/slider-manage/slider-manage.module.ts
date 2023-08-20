import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SliderManageComponent } from './slider-manage.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { RouterModule } from '@angular/router';
import { SliderUploadComponent } from './slider-upload/slider-upload.component';
import { EditSliderComponent } from './edit-slider/edit-slider.component';
import { MobxAngularModule } from 'mobx-angular';

const routes=[
  { path:'', component: SliderManageComponent },
  { path:'upload', component: SliderUploadComponent },
  { path:'edit', component: EditSliderComponent }
]
@NgModule({
  declarations: [
    SliderManageComponent,
    SliderUploadComponent,
    EditSliderComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterializeRFormsModule,
    MobxAngularModule,
    RouterModule.forChild(routes)
  ]
})
export class SliderManageModule { }
