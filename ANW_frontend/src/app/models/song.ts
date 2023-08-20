import { Artist } from "./artist";
import { User } from "./user";

export class Song{
    Id: any;
    Name: string;
    Paths: any;
    Lyrics: any;
    Artists: string[];
    ArtistList: Artist[];
    SubType: string;
    UploadedBy: string;
    RefVideo: string;
    Tags:string;
    Approved:number;
    SelfPerformance: number;
    View: number;
    HitDownload: number;
    HitLove: number;
    UpdatedOn: Date;
    CreatedOn: Date;
    Timer: string;
    Uri: string;
    Duration: number;
    UploadedUser: User;
}