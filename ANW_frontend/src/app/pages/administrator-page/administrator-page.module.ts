import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdministratorPageComponent } from './administrator-page.component';
import { SideMenuModule } from '../../components/side-menu/side-menu.module';
import { Route, RouterModule } from '../../../../node_modules/@angular/router';
import { SongManageModule } from '../songmanage/songmanage.module';
import { ManageArtistModule } from '../artist/manage-artist/manage-artist.module';
import { SliderManageModule } from '../slider-manage/slider-manage.module';
import { AdminNavbarComponent } from '../../components/admin-navbar/admin-navbar.component';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { ResponsiveImageModule } from '../../directives/responsive-image.module';
import { AlbumManagerModule } from '../album-manager/album-manager.module';
import { AuthGuardService } from '../../services/auth-guard.service';
import { AdminGuardService } from '../../services/admin-guard.service';
import { MyPlaylistModule } from '../my-playlist/my-playlist.module';
import { MyFavoriteModule } from '../my-favorite/my-favorite.module';
import { FooterModule } from '../../components/footer/footer.module';
import { SystemConfigComponent } from './system-config/system-config.component';
import { ConfigService } from 'src/app/services/config.service';
import { MobxAngularModule } from 'mobx-angular';
import { FormsModule } from '@angular/forms';
import { GuardLevel } from 'src/app/models/constants/GuardLevel';
import { RankingManagerComponent } from './ranking-manager/ranking-manager.component';
import { RankingService } from 'src/app/services/ranking.service';
import { VideoManageModule } from '../video-manage/video-manage.module';


const routes: Route[]=[
  { path:'', component: AdministratorPageComponent, children:[
    { path:'', redirectTo:'song-manager'},
    { path:'song-manager', loadChildren: ()=> SongManageModule, canActivate: [AuthGuardService], canActivateChild: [AuthGuardService] },
    { path:'album-manager', loadChildren: ()=> AlbumManagerModule, canActivate: [AdminGuardService], canActivateChild: [AdminGuardService] },
    { path:'artist-manager', loadChildren:()=> ManageArtistModule, canActivate: [AdminGuardService], canActivateChild: [AdminGuardService] },
    { path:'banner', loadChildren:()=> SliderManageModule, canActivate:[AdminGuardService], canActivateChild: [AdminGuardService] },
    { path:'myplaylist', loadChildren: ()=>MyPlaylistModule ,canActivate: [AuthGuardService], canActivateChild: [AuthGuardService] },
    { path:'favorite', loadChildren: ()=>MyFavoriteModule, canActivate: [AuthGuardService], canActivateChild: [AuthGuardService] },
    { path:'video-manager', loadChildren: ()=>VideoManageModule, canActivate: [AuthGuardService], canActivateChild: [AuthGuardService] },
    { path:'configure', component: SystemConfigComponent, canActivate:[AdminGuardService], data:{ guardLevel: GuardLevel.ADMINISTRATOR } }
  ] }
];
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    SideMenuModule,
    MaterializeRFormsModule,
    ResponsiveImageModule,
    MobxAngularModule,
    FooterModule,
    RouterModule.forChild(routes)
  ],
  declarations: [
    AdministratorPageComponent,
    AdminNavbarComponent,
    SystemConfigComponent,
    RankingManagerComponent
  ],
  providers:[ ConfigService, RankingService ]
})
export class AdministratorPageModule { }
