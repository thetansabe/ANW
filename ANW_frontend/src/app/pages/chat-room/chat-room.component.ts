import { Component, OnInit, ChangeDetectionStrategy, ViewChild, OnDestroy } from '@angular/core';
import { ChatService } from '../../services/chat.service';
import { environment } from '../../../environments/environment';
import { LanguageService } from '../../services/language.service';
import { Room } from '../../models/room';
import { UserService } from '../../services/user.service';
import { AddChatRoomComponent } from './add-chat-room/add-chat-room.component';
import { ChatLog, ChatContent } from '../../models/chatlog';
import { Subscription } from '../../../../node_modules/rxjs';
import { transition, trigger, style, animate } from '../../../../node_modules/@angular/animations';
import { ConnectionState } from '../../services/signalr.service';

@Component({
  changeDetection: ChangeDetectionStrategy.Default,
  selector: 'app-chat-room',
  templateUrl: './chat-room.component.html',
  styleUrls: ['./chat-room.component.css'],
  animations: [ trigger('chatContent',[
    transition(":enter",[
      style({ opacity: 0 , transform:'scale(0.6)'}),
      animate("0.2s 100ms ease-in-out", style({ opacity: 1, transform:'scale(1)'}))
    ])
  ])
  ]
})
export class ChatRoomComponent implements OnInit, OnDestroy {
  backgroundImage: string="background.png";
  chatContext: string="";
  currentRoom: Room; 
  userData={};
  @ViewChild(AddChatRoomComponent) addingModal: AddChatRoomComponent;
  roomWatcher: Subscription;
  userWatcher: Subscription;
  msgWatcher: Subscription;
  joinerWatcher: Subscription;


  sent: boolean=true;
  sentDate: Date;
  constructor(
    private chatService: ChatService,
    public lang: LanguageService,
    private userService: UserService
  ) { }

  ngOnInit() {
    window.scrollTo(0,0);
    this.chatService.getAll();
    this.roomWatcher=this.chatService.Room.subscribe(room=> {
      
      this.currentRoom=room;
      this.scrollDown();
    });
    this.userWatcher=this.chatService.AllUsers.subscribe(list=>{
        //console.log(list);
        if (list && list.length)
          list.forEach(user=>{
            this.userData[user.Id]=user;
          });
    });
    this.joinerWatcher=this.chatService.onJoinedRoom().subscribe(res=>{
      if (res.newUser && this.currentRoom.ChatLog.findIndex(x=>x.Message && x.Message.includes(res.newUser.DisplayName))<0){
        this.chatService.insertUser(res.newUser);
        const log=new ChatLog();
        log.Content=[];
        log.Message=res.newUser.DisplayName+" "+this.lang.ui.msg_joinedroom;
        this.currentRoom.ChatLog.push(log);
        this.scrollDown();
      }
    })
    this.msgWatcher= this.chatService.onChatReceived().subscribe(chat=>{
      //console.log(chat);  
      this.sent=false;
      let log: ChatLog;
      if (this.currentRoom.ChatLog.length==0){
        log=new ChatLog();
        log.UpdatedOn=new Date();
        log.UserId=chat.sender;
        log.Content=[];
        this.currentRoom.ChatLog.push(log);
      }
      log=this.currentRoom.ChatLog[this.currentRoom.ChatLog.length-1];
      const content=new ChatContent();
      content.Content=chat.message;
      content.CreatedOn=new Date();
      if (log.UserId==chat.sender)
      {
        const current=new Date().getTime()/1000;
        const last=new Date(log.UpdatedOn).getTime()/1000;
        if (current-last<=20 && log.Content.length>=1)
        {
          const latest=log.Content[log.Content.length-1];
          latest.Content+="<br />"+chat.message;
          latest.CreatedOn=content.CreatedOn;
        }
        else{
            log.Content.push(content);
            log.UpdatedOn=new Date();
        }
      }
      else{
        log=new ChatLog();
        log.UpdatedOn=new Date();
        log.UserId=chat.sender;
        log.Content=[];
        this.currentRoom.ChatLog.push(log);
        log.Content.push(content);
      }
      this.scrollDown();
    })
  }

  joinRoom(room: Room){
    if (!this.currentRoom || room.Id!= this.currentRoom.Id){
      this.chatService.join(room.Id);
    }
  }

  submit(){
    if (this.stateDisconnected) return;
    if (!this.chatContext) return;
    if (!this.realChatContent) return;
    const msg=this.chatContext.trim().replace(/\r\n/g,"<br />");
    const userId=this.userService.profile.getValue().Id;
    
    let newLog=true;
    let content: ChatContent= new ChatContent();
    let lastlog:ChatLog=null;
    content.CreatedOn=new Date();
    content.Content=msg;
    content.Error=false;
    content.NotSend=true;
    this.sent=false;
    
    if (this.currentRoom.ChatLog.length>0){
      const log=this.currentRoom.ChatLog[this.currentRoom.ChatLog.length-1];
      if (log!=null && log.UserId==userId)
      {
        const current=new Date().getTime()/1000;
        const last=new Date(log.UpdatedOn).getTime()/1000;
        if (current-last<=20 && log.Content.length>=1)
          lastlog=log;
        else
          log.UpdatedOn=new Date();
        newLog=false;
        log.Content.push(content);
      }
    }
    if (newLog){
      const chatBubble=new ChatLog();
      chatBubble.UserId=userId
      chatBubble.Content=[];
      chatBubble.Content.push(content);
      chatBubble.UpdatedOn=new Date();
      this.currentRoom.ChatLog.push(chatBubble);
    }
    this.chatService.send(msg).subscribe(()=>{
      if (lastlog!=null)
      {
        const previousContent=lastlog.Content.splice(lastlog.Content.length-1,1)[0];
        const mergeContent=lastlog.Content[lastlog.Content.length-1];
        mergeContent.Content+="<br />"+previousContent.Content;
        mergeContent.CreatedOn=previousContent.CreatedOn;
        mergeContent.NotSend=false;
      }
      else
      content.NotSend=false;
      this.sent=true;
      this.sentDate=new Date();
    },err=>{
      content.Error=true;
    });
    this.chatContext="";
    this.scrollDown();
  }

  keyUp(e:KeyboardEvent){
    if (e.keyCode==13)
    {
      if (e.altKey==false)
      {
        this.submit(); 
        e.preventDefault();
      }
      else if (e.altKey==true){
        this.chatContext+="\r\n";
      }
    }
  }

  isSelf(userId: string){
    if (!this.userService.profile.getValue()) return false;
    return this.userService.profile.getValue().Id==userId;
  }

  getAvatar(userId: string){
    if (this.isSelf(userId))
      return this.userAvatar;
    return this.userData[userId] && this.userData[userId].AvatarImg? this.BaseURL+this.userData[userId].AvatarImg
      : '../../../assets/images/icons/empty-user.png';
  }

  get userAvatar(){
    if (!this.userService.profile.getValue() || !this.userService.profile.getValue().AvatarImg) return "";
    return environment.RSRC_URL + this.userService.profile.getValue().AvatarImg;
  }

  get BaseURL(){
    return environment.RSRC_URL;
  }

  get realChatContent(){
    return this.chatContext.trim().replace(/[\r]*[\n]*[\t]*/g,"");
  }

  get isAdmin(){
    return this.userService.IsAuthenticated && this.userService.getUserLevel()>=9;
  }

  get stateDisconnected(){
    return this.chatService.ConnectionState!=ConnectionState.CONNECTED;
  }

  get roomList(){
    return this.chatService.RoomList;
  }
  get ImagePath(){
    return environment.RSRC_URL+this.backgroundImage;
  }

  scrollDown(){
    setTimeout(()=>{
      const target=document.getElementById('chat-box');
      target.scrollTop=target.scrollHeight;
    },100);
  }

  ngOnDestroy(){
    if (this.currentRoom)
      this.chatService.leave(this.currentRoom.Id);
    if (this.roomWatcher) this.roomWatcher.unsubscribe();
    if (this.userWatcher) this.userWatcher.unsubscribe();
    if (this.msgWatcher) this.msgWatcher.unsubscribe();
    if (this.joinerWatcher) this.joinerWatcher.unsubscribe();
  }
}
