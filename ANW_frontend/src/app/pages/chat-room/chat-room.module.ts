import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '../../../../node_modules/@angular/forms';
import { MaterializeMediaModule } from '../../directives/MaterializeR/materialize-module/materialize-media.module';
import { MaterializeRFormsModule } from '../../directives/MaterializeR/materialize-module/materialize-rforms.module';
import { ChatRoomComponent } from './chat-room.component';
import { ResponsiveImageModule } from '../../directives/responsive-image.module';
import { RouterModule } from '../../../../node_modules/@angular/router';
import { NavModule } from '../../components/nav/nav.module';
import { AddChatRoomComponent } from './add-chat-room/add-chat-room.component';
import { EditChatRoomComponent } from './edit-chat-room/edit-chat-room.component';
import { ChatService } from '../../services/chat.service';

const routes=[
  { path: '', component: ChatRoomComponent }
]
@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterializeMediaModule,
    MaterializeRFormsModule,
    ResponsiveImageModule,
    NavModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes)
  ],
  declarations: [ChatRoomComponent, AddChatRoomComponent, EditChatRoomComponent],
  providers: [ ChatService ]
})
export class ChatRoomModule { }
