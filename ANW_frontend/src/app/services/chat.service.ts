import { Injectable, OnInit } from '@angular/core';
import { SignalrService, ConnectionState } from './signalr.service';
import { map, catchError } from '../../../node_modules/rxjs/operators';
import { Room } from '../models/room';
import { HttpClient } from '../../../node_modules/@angular/common/http';
import {environment} from '../../environments/environment';
import { Router } from '../../../node_modules/@angular/router';
import { BehaviorSubject, throwError, Subject } from '../../../node_modules/rxjs';
import { ChatLog } from '../models/chatlog';
import { UserService } from './user.service';
import { LoaderService } from './loader.service';
import { User } from '../models/user';

@Injectable()
export class ChatService{
  private URL: string = environment.HOST+"/api/chat";
  private rooms: Room[];
  private currentRoom: Room;
  private reporter: BehaviorSubject<Room>=new BehaviorSubject<Room>(null);
  private userSub: BehaviorSubject<User[]>=new BehaviorSubject<User[]>(null);
  private userData: User[]=[];
  private state: ConnectionState;
  constructor(
    private signalR: SignalrService,
    private router: Router,
    private http: HttpClient,
    private userService: UserService,
    private loaderService: LoaderService
  ) {
    const roomId=sessionStorage.getItem("currentRoom");
    if (roomId)
      this.join(roomId);
  }

  getAll(){
    this.http.get(this.URL, {headers: this.userService.getAuthorizedHeader()}).pipe(
      map(res=>res as Room[])
    ).subscribe(res=>this.rooms=res, err=>{
      this.router.navigate(['/']);
    });
    this.signalR.on("roomInfo").pipe(
      map(res=>JSON.parse(res+'') as RoomInfoEvent)
    ).subscribe(res=>{
      res.room.ChatLog=res.chatlog;
      this.currentRoom=res.room;
      this.reporter.next(this.currentRoom);
      this.userData=[];
      if (res.joinedUsers)
        res.joinedUsers.forEach(user=>this.userData.push(user));
      this.userSub.next(this.userData);
    });
    this.signalR.connectionState.subscribe(state=> this.state=state);
  }

  save(room: Room){
    if (! this.userService.IsAuthenticated) return null;
    if (room.Id)
      return this.http.put(this.URL+"/room/edit",room, {headers: this.userService.getAuthorizedHeader()})
      .pipe(
        catchError(err=> throwError(err)),
        map(res=>res as Room)
      );
    else
      return this.http.post(this.URL+"/room/create",room, {headers: this.userService.getAuthorizedHeader()} )
      .pipe(
        catchError(err=>throwError(err)),
        map(res=> res as Room)
      );
  }

  join(roomId: string){
    this.loaderService.isLoading=true;
    if (this.signalR.IsAuthorized){
      this.signalR.send("joinRoom",roomId)
      .pipe(
        map(res=>JSON.parse(res) as RoomInfoEvent)
      )
      .subscribe((res)=>{
        this.loaderService.isLoading=false;
        this.currentRoom= res.room;
        this.currentRoom.ChatLog=res.chatlog;
        if (res.joinedUsers)
          res.joinedUsers.forEach(user=>this.userData.push(user));
        this.reporter.next(this.currentRoom);
        this.userSub.next(this.userData);
      });
    }
  }

  leave(roomId: string=null){
    this.userData=[];
    if (!roomId) roomId=this.currentRoom.Id;
    if (roomId){
      if (this.signalR.IsAuthorized)
        this.signalR.send("leaveRoom",roomId);
    }
  }

  onJoinedRoom(){
    return this.signalR.on("onJoined").pipe(
      map(res=> JSON.parse(res+'') as RoomEvent)
    )
  }
  onLeftRoom(){
    return this.signalR.on("onLeft").pipe(
      map(res=> res as RoomEvent)
    )
  }
  onChatReceived(){
    return this.signalR.on("onReceivedMessage").pipe(
      map(res=> JSON.parse(res+'') as ChatEvent)
    )
  }
  send(msg: string){
    if (!this.currentRoom) return;
    return this.signalR.send("sendMessage",msg,this.currentRoom.Id);
  }
  insertUser(user: User){
    if (this.userData.findIndex(x=>x.Id==user.Id)<0)
      this.userData.push(user);
    this.userSub.next(this.userData);
  }

  get RoomList():Room[]{
    if (!this.rooms)
      this.getAll();
    return this.rooms;
  }
  get Room():BehaviorSubject<Room>{
    return this.reporter;
  }

  get AllUsers():BehaviorSubject<User[]>{
    return this.userSub;
  }

  get ConnectionState(){
    return this.state;
  }
}
class RoomEvent{
  userId:string;
  roomId:string;
  newUser?: User;
}
class ChatEvent{
  message: string;
  sender: string;
}
class RoomInfoEvent{
  room: Room;
  chatlog: ChatLog[];
  chatlogLength?: number;
  chatlogOffset?: number=0;
  joinedUsers?: User[];
  newUser?: User;
}