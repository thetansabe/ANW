import { Component, OnInit, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { SimpleTransition } from '../../../components/transitions/simple.transition';
import { LanguageService } from '../../../services/language.service';
import { observable } from '../../../../../node_modules/mobx-angular';
import { FormGroup, FormBuilder, Validators } from '../../../../../node_modules/@angular/forms';
import { Song } from '../../../models/song';
import { MaterializeChipsDirective } from '../../../directives/MaterializeR/materialize-chips.directive';
import { AlbumManagerService } from 'src/app/services/album-manager.service';
import { Album } from 'src/app/models/album';
import { MaterializeToastService } from 'src/app/directives/MaterializeR/materialize-toast.service';
import { Router, ActivatedRoute } from '@angular/router';
import { debounceTime } from 'rxjs/operators';
import { SongService } from 'src/app/services/song.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-create-album',
  templateUrl: './create-album.component.html',
  styleUrls: ['./create-album.component.css'],
  animations: [ SimpleTransition ]
})
export class CreateAlbumComponent implements OnInit {

  @observable form: FormGroup;
  @observable songList: Song[]=[];
  @observable selectedSong: Song[]=[];
  @observable isLoading: boolean=false;



  @observable file: File;
  @observable isSearching: boolean=false;


  @ViewChild(MaterializeChipsDirective) chips: MaterializeChipsDirective;
  constructor(
    public lang: LanguageService,
    private builder: FormBuilder,
    private albumManager: AlbumManagerService,
    private router: Router,
    private actRoute: ActivatedRoute,
    private songService: SongService
  ) { 
  }


  ngOnInit() {
    this.form=this.builder.group({
      name:['', Validators.required],
      desc:'',
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
    const album: Album=new Album();
    album.Name=this.form.get('name').value;
    album.Desc=this.form.get('desc').value;
    album.SongList=this.selectedSong;
    this.isLoading=true;
    this.albumManager.create(album,artists,this.file).subscribe(()=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestsuccess, "green rounded");
      this.router.navigate(['..'],{relativeTo: this.actRoute});
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
          if (!this.selectedSong.some(x=>x.Id==this.songList[i].Id))
          {
            this.selectedSong.push(this.songList[i]);
            this.form.get("filterSong").setValue("");
            this.songList=[];
            return;
          }
      }
    }
  }

  removeSong(song: Song){
    const indx=this.selectedSong.findIndex(s=>s.Id==song.Id);
    this.selectedSong.splice(indx,1);
  }

  addSong(song: Song){
    if (!this.selectedSong.some(x=>x.Id==song.Id))
    {
      this.selectedSong.push(song);
      const index=this.songList.findIndex(x=>x.Id==song.Id);
      this.songList.splice(index,1);
    }
  }

  setFile(event){
    const file=event.target.files[0];
    this.file=file;
  }
}
