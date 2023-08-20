import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MobxAngularModule } from '../../../../node_modules/mobx-angular';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { MusicPlayerComponent } from './music-player.component';
import { OpenPlaylistComponent } from './open-playlist/open-playlist.component';
import { CreatePlaylistComponent } from './create-playlist/create-playlist.component';
import { ResetPlaylistComponent } from './reset-playlist/reset-playlist.component';
import { RouterModule } from '../../../../node_modules/@angular/router';
import { FormsModule, ReactiveFormsModule } from '../../../../node_modules/@angular/forms';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MobxAngularModule,
    MaterializeRFormsModule,
    MaterializeMediaModule,
    RouterModule
  ],
  declarations: [
    MusicPlayerComponent,
    OpenPlaylistComponent,
    CreatePlaylistComponent,
    ResetPlaylistComponent
  ],
  exports:[
    MusicPlayerComponent,
    OpenPlaylistComponent,
    CreatePlaylistComponent,
    ResetPlaylistComponent
  ]
})
export class MusicPlayerModule { }
