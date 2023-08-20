import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ManageArtistComponent } from './manage-artist.component';
import { FormsModule, ReactiveFormsModule } from '../../../../../node_modules/@angular/forms';
import { RouterModule } from '../../../../../node_modules/@angular/router';
import { MaterializeMediaModule } from '../../../directives/MaterializeR/materialize-module/materialize-media.module';
import { MaterializeRFormsModule } from '../../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { EditArtistComponent } from './edit-artist/edit-artist.component';
import { DeleteArtistPromptComponent } from './delete-artist-prompt/delete-artist-prompt.component';
import { ResponsiveImageModule } from '../../../directives/responsive-image.module';
import { MobxAngularModule } from '../../../../../node_modules/mobx-angular';

const routes=[
  { path:'', component: ManageArtistComponent }
]
@NgModule({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MaterializeMediaModule,
    MaterializeRFormsModule,
    FormsModule,
    MobxAngularModule,
    ResponsiveImageModule,
    RouterModule.forChild(routes)
  ],
  declarations: [
    ManageArtistComponent,
    DeleteArtistPromptComponent,
    EditArtistComponent
  ]
})
export class ManageArtistModule { }
