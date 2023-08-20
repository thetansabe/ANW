import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from '../../pages/home/home.component';
import { LoginComponent } from '../../pages/login/login.component';
import { PageNotFoundComponent } from '../../page-not-found/page-not-found.component';
import { DashboardComponent } from '../../pages/dashboard/dashboard.component';
import { AccountComponent } from '../../pages/account/account.component';
import { AuthGuardService } from '../../services/auth-guard.service';
import { AdminGuardService } from '../../services/admin-guard.service';
import { ArtistModule } from '../../pages/artist/artist.module';
import { HomeSongsModule } from '../../pages/home-songs/home-songs.module';
import { SliderManageModule } from '../../pages/slider-manage/slider-manage.module';
import { SongManageModule } from '../../pages/songmanage/songmanage.module';
import { MyPlaylistModule } from '../../pages/my-playlist/my-playlist.module';
import { MyFavoriteModule } from '../../pages/my-favorite/my-favorite.module';
import { MyRegisterModule } from '../../pages/register/register.module';
import { AdministratorBoardModule } from '../../administrator/administrator-board/administrator-board.module';
import { ChatRoomModule } from '../../pages/chat-room/chat-room.module';
import { AdministratorPageModule } from '../../pages/administrator-page/administrator-page.module';
import { VideosModule } from 'src/app/pages/videos/videos.module';

const routes:Routes=[
    { path:'', component: HomeComponent, children:[
      { path:'', redirectTo:'songs', pathMatch:'full'},
      { path: 'songs', loadChildren:()=> HomeSongsModule },
      { path: 'videos', loadChildren:()=> VideosModule }
    ] },
    { path:'management', loadChildren: ()=> AdministratorPageModule, canActivate: [AuthGuardService]},
    { path:'artist', loadChildren: ()=>ArtistModule },
    { path:'login', component: LoginComponent },
    { path:'register',loadChildren:()=>MyRegisterModule},
    { path:'system',loadChildren:()=>AdministratorBoardModule },
    { path:'dashboard', component: DashboardComponent, canActivate:[AuthGuardService] },
    { path:'chat', loadChildren: ()=> ChatRoomModule, canActivate: [AuthGuardService] },
    { path:'account', component: AccountComponent, canActivate:[AuthGuardService] },
    { path:'**', component: PageNotFoundComponent }
];
@NgModule({
  imports: [
    RouterModule.forRoot(routes)
  ],
  exports:[ RouterModule ],
  declarations: []
})
export class AppRouterModule { }
