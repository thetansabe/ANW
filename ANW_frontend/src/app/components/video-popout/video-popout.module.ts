import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VideoPopoutComponent } from './video-popout.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  imports: [
    CommonModule,
    FormsModule
  ],
  declarations: [VideoPopoutComponent],
  exports:[VideoPopoutComponent]
})
export class VideoPopoutModule { }
