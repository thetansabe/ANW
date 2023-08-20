import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VideoManageComponent } from './video-manage.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterializeMediaModule } from 'src/app/directives/MaterializeR/materialize-module/materialize-media.module';
import { MaterializeRFormsModule } from 'src/app/directives/MaterializeR/materialize-module/materialize-rforms.module';
import { RouterModule } from '@angular/router';
import { VideoUploaderComponent } from './video-uploader/video-uploader.component';
import { VideoPopoutModule } from 'src/app/components/video-popout/video-popout.module';

const routes=[
  { path:'', component: VideoManageComponent }
]

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterializeMediaModule,
    MaterializeRFormsModule,
    VideoPopoutModule,
    RouterModule.forChild(routes)
  ],
  declarations: [VideoManageComponent, VideoUploaderComponent]
})
export class VideoManageModule { }
