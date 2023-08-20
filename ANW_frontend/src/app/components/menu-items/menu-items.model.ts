import { locale } from '../../../environments/locale/locale';
export class MenuItem{
    icon: string;
    route?: string;
    title: string;
    desc?: string;
    displayLevel?: number; // scale from 0-10
    displayUntil?: number; // scale from 0-10
    hideOnDashboard?: boolean; // scale from 0-10
}

export const DashboardItemList:MenuItem[]=[
    { icon: 'library_music', route:'/management/song-manager', title: locale.language.songmanage , desc: locale.language.songmanage_desc },
    { icon: 'playlist_add_check', route:'/management/myplaylist', title: locale.language.playlistmanage , desc: locale.language.playlistmanage_desc },
    { icon: 'music_video', route:'/management/video-manager', title: locale.language.videomanager , desc: locale.language.videomanager_desc },
    { icon: 'favorite_border', route:'/management/favorite', title: locale.language.myfavorite , desc: locale.language.myfavorite_desc },
    { icon: 'album', route:"/management/album-manager", title: locale.language.albummanage , desc: locale.language.albummanage_desc, displayLevel: 9 },
    { icon: 'burst_mode', route:'/management/banner', title: locale.language.banneradsmanage , desc: locale.language.banneradsmanage_desc, displayLevel: 9 },
    { icon: 'recent_actors', route:'/management/artist-manager', title: locale.language.artistmanage , desc: locale.language.artistmanage_desc, displayLevel: 9 },
    { icon: 'settings', route:'/management/configure', title: locale.language.systemconfig , desc: "", displayLevel: 10, hideOnDashboard: true }
]
