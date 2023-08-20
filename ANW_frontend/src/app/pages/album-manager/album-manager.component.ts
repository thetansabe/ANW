import { Component, OnInit, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { LanguageService } from '../../services/language.service';
import { FormGroup, FormBuilder } from '../../../../node_modules/@angular/forms';
import { observable } from '../../../../node_modules/mobx-angular';
import { Album } from '../../models/album';
import { ActivatedRoute, Router } from '../../../../node_modules/@angular/router';
import { debounceTime } from '../../../../node_modules/rxjs/operators';
import { AlbumManagerService } from '../../services/album-manager.service';
import { SimpleTransition } from '../../components/transitions/simple.transition';
import { environment } from '../../../environments/environment';
import { LoaderService } from 'src/app/services/loader.service';
import { Song } from 'src/app/models/song';
import { Artist } from 'src/app/models/artist';
import { DeleteAlbumComponent } from './delete-album/delete-album.component';
import { EditAlbumComponent } from './edit-album/edit-album.component';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-album-manager',
  templateUrl: './album-manager.component.html',
  styleUrls: ['./album-manager.component.css'],
  animations:[SimpleTransition]
})
export class AlbumManagerComponent implements OnInit {
  @observable list: Album[];
  @observable currentPage: number;
  @observable pages: number[]=[];
  @observable maxPage: number=1;
  @observable filterForm: FormGroup;
  @observable isLoading: boolean=false;
  @observable q: string;
  @observable selected: boolean[]=[];

  maxSize: number=25;

  @ViewChild(DeleteAlbumComponent) modal: DeleteAlbumComponent;
  @ViewChild(EditAlbumComponent) editor: EditAlbumComponent;
  constructor(
    public lang: LanguageService,
    private loader: LoaderService,
    private builder: FormBuilder,
    private actRoute: ActivatedRoute,
    private router: Router,
    private albManager: AlbumManagerService
  ) { }

  ngOnInit() {
    this.filterForm=this.builder.group({
      filterInput:''
    });
    this.actRoute.queryParams
    .subscribe(params=>{
      if (params['q']){
        this.q=params['q'];
        this.filterForm.get('filterInput').setValue(this.q);
      }
      if (!params['page'])
      {
        this.currentPage=1;
        this.fetch(true);
      }
      else{
        this.currentPage=+params['page'];
        this.fetch();
      }
    })
    this.onSearching();
  }

  onSearching(){
    this.filterForm.get('filterInput').valueChanges.pipe(
      debounceTime(400)
    ).subscribe(val=>{
      if (val!=this.q)
      {
        if (this.q)
          this.router.navigate(['.'], {relativeTo: this.actRoute, queryParams:{ q: this.q } });
        else 
          this.router.navigate(['.'], {relativeTo: this.actRoute });
      }
    })
  }

  fetch(ignore: boolean=false){
    if (this.currentPage<=0) return;
    if (this.currentPage>this.maxPage && !ignore) return;
    this.isLoading=true;
    this.loader.isSubLoading=true;
    this.albManager.get(this.currentPage, this.maxSize, this.q)
    .subscribe(res=>{
      this.isLoading=false;
      this.loader.isSubLoading=false;
      this.clearSelect(res.list);
      this.list=res.list;
      if (this.maxPage!=res.maxPage){
        this.maxPage=res.maxPage;
        this.pages=[];
        for (var i=1; i<=this.maxPage; i++)
          this.pages.push(i);
      }
    },err =>{
      this.isLoading=false;
      this.loader.isSubLoading=false;
      if (err.status==404 || err.status==203){
        this.list=[];
      }
    })
  }

  openDeleteModal(){
    const list:Album[]=[];
    for(var i=0; i<this.selected.length;i++)
      if (this.selected[i])
        list.push(this.list[i]);
    this.modal.open(list);
  }

  edit(album:Album){
    this.editor.open(album);
  }

  onDeleted(list: Artist[]){
    list.forEach(alb=>{
      const idx=this.list.findIndex(x=>x.Id==alb.Id);
      this.list.splice(idx,1);
    })
  }


  toggleChecked(){
    const checker=this.isCheckedAll;
    for (var i=0; i< this.selected.length; i++)
      this.selected[i]=!checker;
  }

  get hasChecked(){
    return this.selected && this.selected.some(x=>x==true);
  }

  get isCheckedAll(){
    return this.selected && this.selected.every(x=>x==true);
  }

  clearSelect(list: Album[]=this.list){
    if (this.selected.length>list.length)
      this.selected=[];
    for (var i=0; i<this.selected.length;i++)
      this.selected[i]=false;
    while (this.selected.length<list.length)
      this.selected.push(false);
  }


  get MEDIA_URL(){
    return environment.MEDIA_URL;
  }

  get backpage(){
    if (this.q)
      return { page: this.currentPage>1?this.currentPage-1:1, q: this.q }
    return { page: this.currentPage>1?this.currentPage-1:1 }
  }
  get nextpage(){
    if (this.q)
      return { page: this.currentPage<this.maxPage?this.currentPage+1:this.maxPage, q: this.q }
    return { page: this.currentPage<this.maxPage?this.currentPage+1:this.maxPage }
  }

  goto(p: number){
    if (this.q)
      return { page: p, q: this.q }
    return { page: p }
  }
}
