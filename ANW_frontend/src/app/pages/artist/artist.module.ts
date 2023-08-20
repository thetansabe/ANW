import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ArtistComponent } from './artist.component';
import { ArtistDetailsComponent } from './artist-details/artist-details.component';
import { MainSliderModule } from '../../components/main-slider/main-slider.module';
import { NavModule } from '../../components/nav/nav.module';
import { RouterModule } from '@angular/router';
import { ListArtistComponent } from './list-artist/list-artist.component';
import { ResponsiveImageModule } from '../../directives/responsive-image.module';
import { FormsModule } from '@angular/forms';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { MobxAngularModule } from 'mobx-angular';
import { FooterModule } from '../../components/footer/footer.module';

const routes=[
  { path:'', component: ArtistComponent, children:[
    { path: '', component: ListArtistComponent }
    //{ path:'manage', component: ManageArtistComponent, canActivate:[AdminGuardService] }
  ] },
  { path:'detail/:id', component: ArtistDetailsComponent },
]
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterializeRFormsModule,
    ResponsiveImageModule,
    MainSliderModule,
    NavModule,
    FooterModule,
    MobxAngularModule,
    RouterModule.forChild(routes)
  ],
  declarations: [
    ArtistComponent, 
    ArtistDetailsComponent,
    ListArtistComponent
  ]
})
export class ArtistModule { }
