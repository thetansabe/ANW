import { Song } from "./song";

export class Playlist{
    Id:string;
    UserId: string;
    Name: string;
    ImagePath: string;
    Collection: string[];
    CreatedOn: Date;
    View: number;
    Public: number;
}

export class PlaylistPageResponse{
    list: Playlist[];
    currentPage: number;
    maxPage: number;
}
export class PlaylistResponse{
    playlist: Playlist;
    songlist: Song[];
}