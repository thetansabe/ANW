import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ViewpageComponent } from './viewpage/viewpage.component';
import { RouterModule } from '@angular/router';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';
import { UploadSongComponent } from './upload-song/upload-song.component';
import { DeleteSongComponent } from './delete-song/delete-song.component';
import { MobxAngularModule } from 'mobx-angular';
import { EditSongComponent } from './edit-song/edit-song.component';


const routes=[
  { path: '', redirectTo:'page/1', pathMatch:'full'},
  { path:'page/:pageNumber', component: ViewpageComponent },
  { path:'upload', component: UploadSongComponent }
]
@NgModule({
  declarations: [
      ViewpageComponent,
      UploadSongComponent,
      DeleteSongComponent,
      EditSongComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterializeRFormsModule,
    MaterializeMediaModule,
    MobxAngularModule,
    RouterModule.forChild(routes)
  ],
})
export class SongManageModule { }
