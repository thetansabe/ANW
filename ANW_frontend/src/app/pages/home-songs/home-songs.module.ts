import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { HomeSongsComponent } from './home-songs.component';
import { RouterModule } from '@angular/router';
import { ChartJsModule } from '../../directives/ChartJS/chart-js.module';
import { SongDetailComponent } from './song-detail/song-detail.component';
import { ResponsiveImageModule } from '../../directives/responsive-image.module';
import { MobxAngularModule } from 'mobx-angular';
import { RankingService } from 'src/app/services/ranking.service';
import { SearchResultComponent } from './search-result/search-result.component';
import { LookupService } from 'src/app/services/lookup.service';


const route=[
  { path:'', component: HomeSongsComponent },
  { path:'search', component: SearchResultComponent },
  { path:'detail/:id', component: SongDetailComponent }
];
@NgModule({
  imports: [
    CommonModule,
    MaterializeRFormsModule,
    ResponsiveImageModule,
    ChartJsModule,
    MobxAngularModule,
    RouterModule.forChild(route)
  ],
  declarations: [
    HomeSongsComponent,
    SongDetailComponent,
    SearchResultComponent
  ],
  providers:[ RankingService, LookupService ]
})
export class HomeSongsModule { }
