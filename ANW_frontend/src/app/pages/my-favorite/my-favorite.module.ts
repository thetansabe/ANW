import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MyFavoriteComponent } from './my-favorite.component';
import { FormsModule } from '../../../../node_modules/@angular/forms';
import { RouterModule } from '../../../../node_modules/@angular/router';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';
import { MobxAngularModule } from 'mobx-angular';

const routes=[
  { path:'', component: MyFavoriteComponent }
]
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterializeMediaModule,
    MobxAngularModule,
    RouterModule.forChild(routes)
  ],
  declarations: [
    MyFavoriteComponent
  ]
})
export class MyFavoriteModule { }
