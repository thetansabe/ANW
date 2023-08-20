import { Artist } from "./artist";
import { Song } from "./song";

export class Album{
    Id: string;
    Name: string;
    Desc: string;
    AvatarImg: string;
    BackgroundImg: string;
    Approved: number;
    View: number;
    UpdatedOn: Date;
    CreatedOn: Date;
    Tags: string;
    IsDeleted: Boolean;

    ArtistList: Artist[];
    SongList: Song[];
}