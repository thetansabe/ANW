import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdministratorBoardComponent } from './administrator-board.component';
import { RouterModule } from '@angular/router';

const route=[
  {path:'initializer' , component: AdministratorBoardComponent}
];
@NgModule({
  imports: [
    CommonModule,
    RouterModule.forChild(route)
  ],
  declarations: [
    AdministratorBoardComponent
  ]
})
export class AdministratorBoardModule { }
