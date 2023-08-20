import { Component, OnInit, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { observable } from 'mobx-angular';
import { Album } from '../../../models/album';
import { environment } from '../../../../environments/environment';
import { LanguageService } from '../../../services/language.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Song } from '../../../models/song';
import { debounceTime } from 'rxjs/operators';
import { SongService } from '../../../services/song.service';
import { SimpleTransition } from '../../../components/transitions/simple.transition';
import { MaterializeChipsDirective, materializeChip } from '../../../directives/MaterializeR/materialize-chips.directive';
import { AlbumManagerService } from '../../../services/album-manager.service';
import { MaterializeToastService } from '../../../directives/MaterializeR/materialize-toast.service';
import { MaterializeModalDirective } from '../../../directives/MaterializeR/materialize-modal.directive';
import { AlbumService } from '../../../services/album.service';
import { MaterializeText } from '../../../directives/MaterializeR/materialize-text';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-edit-album',
  templateUrl: './edit-album.component.html',
  styleUrls: ['./edit-album.component.css'],
  animations:[SimpleTransition]
})
export class EditAlbumComponent implements OnInit {
  
  @observable album: Album;
  @observable songList: Song[];
  @observable isLoading: boolean=false;
  @observable isSearching: boolean=false;

  @ViewChild(MaterializeChipsDirective) chips: MaterializeChipsDirective;
  @ViewChild(MaterializeModalDirective) modal: MaterializeModalDirective;

  @observable form: FormGroup;
  constructor(
    public lang: LanguageService,
    private builder: FormBuilder,
    private songService: SongService,
    private albumManager: AlbumManagerService,
    private albumService: AlbumService
  ) { }

  ngOnInit() {
    this.form=this.builder.group({
      name:['', Validators.required],
      desc: '',
      filterSong:''
    });
    this.onSearching();
  }

  onSearching(){
    this.form.get('filterSong').valueChanges.pipe(
      debounceTime(250)     
    ).subscribe(val=>{
      this.isSearching=true;
      this.songService.find(val,5).subscribe(res=>{
        this.songList=res;
        this.isSearching=false;
      },err=>{
        this.isSearching=false;
      })
    })
  }

  submit(){
    const artists: string[]=[];
    this.chips.tags.forEach(tag=> artists.push(tag.tag));
    this.album.Name=this.form.get('name').value;
    this.album.Desc=this.form.get('desc').value;
    this.isLoading=true;
    this.albumManager.save(this.album,artists).subscribe(()=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestsuccess, "green rounded");
      this.modal.close();
    },err =>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,"red rounded");
    })
  }

  onFilterKeyPress(e: KeyboardEvent){
    if (e.isTrusted){
      if (e.keyCode==13){
        e.preventDefault();
        for (var i =0; i<this.songList.length; i++)
          if (!this.album.SongList.some(x=>x.Id==this.songList[i].Id))
          {
            this.add(this.songList[i]);
            this.form.get("filterSong").setValue("");
            this.songList=[];
            return;
          }
      }
    }
  }

  remove(song: Song){
    const indx=this.album.SongList.findIndex(x=>x.Id==song.Id);
    this.album.SongList.splice(indx,1);
  }

  add(song: Song){
    if (!this.album.SongList)
      this.album.SongList=[];
    if (this.album.SongList.findIndex(x=>x.Id==song.Id)<0)
      this.album.SongList.push(song);
  }

  open(alb: Album){
    this.album=alb;
    this.form.get("name").setValue(alb.Name);
    this.form.get("desc").setValue(alb.Desc);
    this.form.get("filterSong").setValue("");
    this.albumService.get(alb.Id).subscribe(res=>{
      this.album.SongList=res.SongList;
      this.album.ArtistList=res.ArtistList;
      alb.ArtistList.forEach(artist=>{
        const chip: materializeChip=new materializeChip();
        chip.tag=artist.Name;
        if (artist.AvatarImg)
          chip.image=this.URL+artist.AvatarImg;
        this.chips.add(chip);
      });
      MaterializeText.render();
      this.chips.update();
    })
    this.modal.open();
  }

  get URL(){
    return environment.MEDIA_URL;
  }

  get RESOURCE_URL(){
    return environment.RSRC_URL;
  }
}
