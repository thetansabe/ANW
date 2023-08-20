//import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppComponent } from './app.component';
import { AppRouterModule } from './router/app-router/app-router.module';
import { HomeComponent } from './pages/home/home.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LoginComponent } from './pages/login/login.component';
import { HttpClientModule } from '@angular/common/http';
import { AccountComponent } from './pages/account/account.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { MusicPlayerComponent } from './components/music-player/music-player.component';
import { MaterializeRFormsModule } from './directives/MaterializeR/materialize-module/materialize-rforms.module';
import { MaterializeMediaModule } from './directives/MaterializeR/materialize-module/materialize-media.module';
import { CreatePlaylistComponent } from './components/music-player/create-playlist/create-playlist.component';
import { ResetPlaylistComponent } from './components/music-player/reset-playlist/reset-playlist.component';
import { OpenPlaylistComponent } from './components/music-player/open-playlist/open-playlist.component';
import { MainSliderModule } from './components/main-slider/main-slider.module';
import { NavModule } from './components/nav/nav.module';
import { ResponsiveImageModule } from './directives/responsive-image.module';
import { MobxAngularModule } from 'mobx-angular';
import { FooterModule } from './components/footer/footer.module';
import { CookiePolicyComponent } from './components/cookie-policy/cookie-policy.component';
import { CommonModule } from '../../node_modules/@angular/common';
import { MusicPlayerModule } from './components/music-player/music-player.module';
import { FacebookModule } from 'ngx-facebook';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    PageNotFoundComponent,
    LoginComponent,
    AccountComponent,
    DashboardComponent,
    CookiePolicyComponent
  ],
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    MaterializeRFormsModule,
    MaterializeMediaModule,
    MainSliderModule,
    NavModule,
    FooterModule,
    ResponsiveImageModule,
    MobxAngularModule,
    MusicPlayerModule,
    AppRouterModule,
    FacebookModule.forRoot()
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
