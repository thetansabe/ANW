import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VideosComponent } from './videos.component';
import { MaterializeRFormsModule } from 'src/app/directives/MaterializeR/materialize-module/materialize-rforms.module';
import { ResponsiveImageModule } from 'src/app/directives/responsive-image.module';
import { MobxAngularModule } from 'mobx-angular';
import { VideoPopoutModule } from 'src/app/components/video-popout/video-popout.module';
import { RouterModule } from '@angular/router';
import { VideoDetailComponent } from './video-detail/video-detail.component';

const routes=[
  { path:'', component: VideosComponent},
  { path:'detail/:id', component: VideoDetailComponent}
]
@NgModule({
  imports: [
    CommonModule,
    MobxAngularModule,
    MaterializeRFormsModule,
    VideoPopoutModule,
    ResponsiveImageModule,
    RouterModule.forChild(routes)
  ],
  declarations: [VideosComponent, VideoDetailComponent]
})
export class VideosModule { }
