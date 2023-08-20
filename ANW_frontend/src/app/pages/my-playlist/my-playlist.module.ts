import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { MyPlaylistComponent } from './my-playlist.component';
import { RouterModule } from '../../../../node_modules/@angular/router';
import { DeletePlaylistComponent } from './delete-playlist/delete-playlist.component';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';

const routes=[
  { path:'', component: MyPlaylistComponent }
];
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterializeRFormsModule,
    MaterializeMediaModule,
    RouterModule.forChild(routes)
  ],
  declarations: [
    MyPlaylistComponent,
    DeletePlaylistComponent
  ]
})
export class MyPlaylistModule { }
