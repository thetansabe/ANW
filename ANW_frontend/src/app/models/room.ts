import { ChatLog } from "./chatlog";

export class Room{
    Id:string;
    Name:string;
    Capacity:number;
    CreatedBy:string;
    AllowChat: number;
    AllowVoice:number;
    QueueMode: number;
    Publicity: number;
    JoinedUsers: string[];
    ChatLog: ChatLog[];
    Icon: string;
    Color: string;
}