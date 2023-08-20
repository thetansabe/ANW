import { Component, OnInit, ViewChild, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { Artist } from '../../../models/artist';
import { ArtistService } from '../../../services/artist.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ListTransition } from '../../../components/transitions/list.transition';
import { DeleteArtistPromptComponent } from './delete-artist-prompt/delete-artist-prompt.component';
import { computed, observable } from 'mobx-angular';
import { EditArtistComponent } from './edit-artist/edit-artist.component';
import { FormBuilder, FormGroup } from '../../../../../node_modules/@angular/forms';
import { debounceTime } from '../../../../../node_modules/rxjs/operators';
import { LoaderService } from 'src/app/services/loader.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-manage-artist',
  templateUrl: './manage-artist.component.html',
  styleUrls: ['./manage-artist.component.css'],
  animations:[ ListTransition ]
})
export class ManageArtistComponent implements OnInit {
  artists: Artist[]=[];
  @observable page: number;
  maxPage: number=1;
  pages: number[]=[];
  @observable selected: boolean[]=[];
  @observable keyword: string;


  baseUrl: string ="/management/artist-manager";

  isLoading: boolean=false;

  @ViewChild(DeleteArtistPromptComponent) modal: DeleteArtistPromptComponent;
  @ViewChild(EditArtistComponent) editor: EditArtistComponent;
  private bound: number=6;
  private limit: number=12;

  @observable filterForm: FormGroup;

  constructor(
    public lang: LanguageService,
    private loader: LoaderService,
    private artistService: ArtistService,
    private actRoute: ActivatedRoute,
    private router: Router,
    private builder: FormBuilder,
    private watcher: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.filterForm=this.builder.group({
      filterInput: ['']
    });
    this.actRoute.queryParams.subscribe(params=>{
      this.keyword=params['keyword'] || null;
      if (this.keyword) this.filterForm.get('filterInput').setValue(this.keyword);
      if (params['page'])
      {
        this.loader.isSubLoading=true;
        this.page=+params['page'];
        if (this.page==1){
          this.setPage(1,true);
        }
        else
          this.setPage(this.page);

      } 
      else {
        window.scrollTo(0,0);
        this.router.navigate([this.baseUrl],{queryParams:{page:1}});
      }
    })
    this.onSearching();
  }

  onSearching(){
    this.filterForm.get('filterInput').valueChanges.pipe(debounceTime(400))
    .subscribe(val=>{
      if (val)
      {
        this.router.navigate([this.baseUrl], { queryParams: { page:1, keyword: val }});
      }
      else this.router.navigate([this.baseUrl], { queryParams: { page:1 }});
    })
  }

  setPage(p: number=1, skipCheck: boolean=false){
    if ((p<=0 || p> this.maxPage) && !skipCheck) return;
    if (this.page!=p)
    {
      this.router.navigate([this.baseUrl],{queryParams:{page:p}});
      return;
    }
    this.isLoading=true;
    this.artistService.getToManage(p,this.limit, this.keyword).subscribe(res=>{
      this.isLoading=false;
      this.loader.isSubLoading=false;
      if (res==null || res.maxPage==0)
      {
        this.artists=[];
        this.maxPage=1;
        this.page=1;
        return;
      }
      this.clearSelected(res.list.length);
      if (this.page==res.page && res.list.length>this.artists.length){
        const arr=(res.list.filter(x=> !this.artists.some(s=>s.Id==x.Id)));
        arr.forEach(item=> this.artists.push(item));
      }
      else
        this.artists=res.list;
      this.page=res.page;
      this.maxPage=res.maxPage;
      this.calcPages();
    }, err=>{
      if (err.status==404)
        this.router.navigate(['/notfound']);
    })

  }

  calcPages(){
    this.pages=[];
    for (var i=1; i<=this.maxPage; i++){
      this.pages.push(i);
    }
    const divide=this.bound/2;
    const offset=this.page-1<divide?0:this.page-1-divide;
    this.pages=this.pages.splice(offset,this.bound);
    this.watcher.detectChanges();
  }

  clearSelected(length: number=this.artists.length){
    if (this.selected.length>length)
      this.selected=[];
    for (var i=0; i<this.selected.length; i++)
      this.selected[i]=false;
    while (this.selected.length<length)
      this.selected.push(false);
  }

  onSelectChanged(){
    const check=this.isCheckedAll;
    for (var i=0; i<this.artists.length; i++)
      this.selected[i]=!check;
  }

  get isCheckedAll(){
    return this.selected.length==this.artists.length && this.selected.every(x=>x==true);
  }

  get hasChecked(){
    return this.selected.length>0 && this.selected.some(x=>x);
  }

  delete(){
    if (this.selected && this.hasChecked){
      const list: Artist[]=[];
      for (var i=0; i< this.selected.length;i++)
        if (this.selected[i])
          list.push(this.artists[i]);
      this.modal.open(list);
    }
  }

  onDeleted(e: Artist[]){
    if (e && e.length>0)
    {
      e.forEach(artist=>{
        const index=this.artists.findIndex(x=>x.Id== artist.Id);
        if (index>=0)
          this.artists.splice(index,1);
        
      })
      this.setPage(this.page,true);
    }
  }

  onSaveChanges(e: Artist){
    if (e){
      const target=this.artists.find(x=>x.Id==e.Id);
      target.CountryName=e.CountryName;
      target.Country=e.Country;
      target.Name=e.Name;
      target.DateOfBirth=e.DateOfBirth;
      target.Desc=e.Desc;
    }
  }

  edit(artist: Artist){
    this.editor.open(artist);
  }

  get backpage(){
    if (this.keyword)
      return { page: this.page<=1?1:this.page-1, keyword: this.keyword};
    return { page: this.page<=1?1:this.page-1 };
  }

  goto(p: number){
    if (this.keyword)
      return { page: p, keyword: this.keyword};
    return { page: p};
  }

  get nextpage(){
    if (this.keyword)
      return { page: this.page>=this.maxPage?this.maxPage:this.page+1, keyword: this.keyword };
    return { page: this.page>=this.maxPage?this.maxPage:this.page+1 };
  }
}
