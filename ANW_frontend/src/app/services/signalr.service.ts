import { Injectable } from '@angular/core';
import { HubConnectionBuilder, HubConnection, LogLevel } from '@aspnet/signalr';
import { MessagePackHubProtocol } from '@aspnet/signalr-protocol-msgpack';
import { environment } from '../../environments/environment';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { UserService } from './user.service';
import { MaterializeToastService } from '../directives/MaterializeR/materialize-toast.service';
import { LanguageService } from './language.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  
  private hub: HubConnection;
  private url= environment.HOST;
  private isConnected:boolean;
  private maxRetries:number=5;
  private retries: number=0;
  private connectReport: BehaviorSubject<ConnectionState>=new BehaviorSubject<ConnectionState>(ConnectionState.NOTCONNECTED);
  private authorizing: boolean=false;
  private authorized: boolean=false;
  constructor(
    private user: UserService,
    private lang: LanguageService
  ) { 
    this.connect();
  }

  connect(){
    if (this.isConnected) return;
    if (!this.user.IsAuthenticated){
      this.authorized=false;
      this.hub=new HubConnectionBuilder()
      .withUrl(this.url+"/signalr")
      .configureLogging(environment.LogLevel)
      .withHubProtocol(new MessagePackHubProtocol())
      .build();
    }
    else{
      this.authorized=true;
      this.hub=new HubConnectionBuilder()
      .withUrl(this.url+"/signalr",{
        accessTokenFactory:()=>{
          return this.user.getToken()
        }
      })
      .configureLogging(environment.LogLevel)
      .withHubProtocol(new MessagePackHubProtocol())
      .build();
    }
    this.isConnected=false;
    const self=this;
    this.hub.start()
    .then(()=>
    {
      if (self.retries>0){
        if (!self.authorizing)
          MaterializeToastService.send(this.lang.ui.signalr_msg_connected,"green");
      }
      self.isConnected=true;
      self.connectReport.next(ConnectionState.CONNECTED);
      if (self.retries>=2)
      {
        window.location.reload();
      }
      self.authorizing=false;
      self.retries=0;
    }).catch(err=>{
      self.isConnected=false;
      self.connectReport.next(ConnectionState.DISCONNECTED);
      if (!self.authorizing){
        if (self.retries<=self.maxRetries){
          self.connectReport.next(ConnectionState.RETRYING);
            MaterializeToastService.send(this.lang.ui.signalr_msg_reconnecting,"red",5500+(self.retries*500));
          setTimeout(()=>self.connect(),5000+(self.retries*500));
          self.retries++;
        }
        else {
            MaterializeToastService.send(this.lang.ui.signalr_msg_disconnected,"red",8000);
        }
      }
      // console.error(err);
    });
    this.hub.onclose((err)=>{
      self.isConnected=false;
      self.connectReport.next(ConnectionState.DISCONNECTED);
      if (!self.authorizing){
        if (self.retries<=self.maxRetries){
          self.connectReport.next(ConnectionState.RETRYING);
          MaterializeToastService.send(this.lang.ui.signalr_msg_reconnecting,"red",5500+(self.retries*500));
          setTimeout(()=>self.connect(),5000+(self.retries*500));
          self.retries++;
        }
        else {
          MaterializeToastService.send(this.lang.ui.signalr_msg_disconnected,"red",8000);
        }
      }
    })
  }

  on(event: string): Observable<{}>{
    const result=new Subject();
    this.hub.on(event, (data)=>result.next(data));
    return result.asObservable();
  }

  call(method: string, ...data: any[]): Observable<{}>{
    const result=new Subject();
    this.hub.send(method, data).then(
      ()=>result.next("")
    ).catch(err=>result.error(err));
    return result.asObservable();
  }
  
  send(method: string, data: string, data2: string=null, data3: string=null): Observable<any>{
    const result=new Subject();
    if (!data2 && !data3)
      this.hub.invoke(method, data).then(data=>{
        const status=data.status;
        const statusText=(data.status+'').toUpperCase();
        if (status==200 || status==204 || statusText=="OK" || statusText=="NO_CONTENT")
          result.next(data.data);
        else result.error(data);
    },err=>result.error(err));
    else if (!data3)
      this.hub.invoke(method, data,data2).then(data=>{
        const status=data.status;
        const statusText=(data.status+'').toUpperCase();
        if (status==200 || status==204 || statusText=="OK" || statusText=="NO_CONTENT")
          result.next(data.data);
        else result.error(data);
      },err=>result.error(err));
    else 
      this.hub.invoke(method, data,data2,data3).then(data=>{
        const status=data.status;
        const statusText=(data.status+'').toUpperCase();
        if (status==200 || status==204 || statusText=="OK" || statusText=="NO_CONTENT")
          result.next(data.data);
        else result.error(data);
      },err=>result.error(err));
    return result.asObservable();
  }

  sendAuthorize(){
    if (this.user.IsAuthenticated){
      this.authorizing=true;
      this.reconnect();
    }
  }
  reconnect(){
    var self=this;
    this.hub.stop().then(()=>{
      self.isConnected=false;
      self.connect();
    });
  }

  get connectionState(): Observable<ConnectionState>{
    return this.connectReport.asObservable();
  }

  get IsAuthorized(){
    if (!this.user.IsAuthenticated && this.authorized)
      this.reconnect();
    return this.user.IsAuthenticated;
  }
}
export enum ConnectionState{
  DISCONNECTED, NOTCONNECTED, CONNECTED, RETRYING
}