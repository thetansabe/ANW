import { Artist } from "./artist";
import { User } from "./user";

export class Video{
    Id: any;
    Name: string;
    Path: string;
    Extension: string;
    Thumbnail: string;
    ThumbnailExtension: string;
    Artists: string[];
    ArtistList: Artist[];
    Type: string;
    UploadBy: string;
    Tags:string;
    Approve:number;
    View: number;
    UpdatedOn: Date;
    CreatedOn: Date;
    Uploader: User;
}