import { Component, OnInit, ViewChild } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { FormGroup, FormBuilder, Validators } from '../../../../../node_modules/@angular/forms';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { Room } from '../../../models/room';
import { ChatService } from '../../../services/chat.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';

@Component({
  selector: 'app-add-chat-room',
  templateUrl: './add-chat-room.component.html',
  styleUrls: ['./add-chat-room.component.css']
})
export class AddChatRoomComponent implements OnInit {

  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;
  form: FormGroup;
  isLoading=false;
  iconList: any=[ 
    { ico:"home", desc: this.lang.ui.ico_home },
    { ico:"favorite", desc: this.lang.ui.ico_heart },
    { ico:"face", desc: this.lang.ui.ico_face },
    { ico:"star_rate", desc: this.lang.ui.ico_star },
    { ico:"group", desc: this.lang.ui.ico_group },
    { ico:"work", desc: this.lang.ui.ico_school },
    { ico:"surround_sound", desc: this.lang.ui.ico_radio },
    { ico:"chat", desc: this.lang.ui.ico_chat },
    { ico:"pin_drop", desc: this.lang.ui.ico_travel },
    { ico:"fastfood", desc: this.lang.ui.ico_food } ];
  constructor(
    public lang: LanguageService,
    private builder: FormBuilder,
    private chatService:ChatService
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      roomName:['',Validators.required],
      icon: 'home',
      cap: ['0', Validators.pattern(/[0-9]*/g)],
      publicity: '0'
    })
  }

  save(){
    this.isLoading=true;
    let r: Room=new Room();
    r.Name=this.form.get('roomName').value;
    r.Capacity=this.form.get('cap').value;
    r.Icon=this.form.get('icon').value;
    r.Publicity=+this.form.get('publicity').value;
    r.AllowChat=1;
    this.chatService.save(r).subscribe(res=>{
      this.isLoading=false;
      this.modal.close();
      this.chatService.RoomList.push(res);
    },err=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,"red rounded");
    });
  }

  open(){
    this.form.get('roomName').setValue('');
    this.modal.open();
  }

}
