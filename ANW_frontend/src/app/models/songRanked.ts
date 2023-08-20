import { Song } from './song';

export class SongRanked{
    SongId: string;
    Priority: number;
    ValidFrom: Date;
    ValidTo: Date;
    LastUpdated: Date;
    Song: Song;
}