import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlbumManagerComponent } from './album-manager.component';
import { FormsModule, ReactiveFormsModule } from '../../../../node_modules/@angular/forms';
import { Route, RouterModule } from '../../../../node_modules/@angular/router';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { AlbumManagerService } from '../../services/album-manager.service';
import { MobxAngularModule } from '../../../../node_modules/mobx-angular';
import { CreateAlbumComponent } from './create-album/create-album.component';
import { DeleteAlbumComponent } from './delete-album/delete-album.component';
import { EditAlbumComponent } from './edit-album/edit-album.component';
import { ResponsiveImageModule } from '../../directives/responsive-image.module';

const routes: Route[]=[
  { path:'', component: AlbumManagerComponent },
  { path: 'create', component: CreateAlbumComponent }
]
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MobxAngularModule,
    MaterializeRFormsModule,
    ResponsiveImageModule,
    RouterModule.forChild(routes)
  ],
  declarations: [AlbumManagerComponent, CreateAlbumComponent, DeleteAlbumComponent, EditAlbumComponent],
  providers:[
    AlbumManagerService
  ]
})
export class AlbumManagerModule { }
