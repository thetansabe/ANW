import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ArtistService } from '../../../services/artist.service';
import { Artist } from '../../../models/artist';
import { Subscription } from 'rxjs';
import { LoaderService } from '../../../services/loader.service';
import { LanguageService } from '../../../services/language.service';
import { transition, trigger, query, style, animate, stagger, sequence } from '@angular/animations';
import { environment } from '../../../../environments/environment';


@Component({
  selector: 'app-list-artist',
  templateUrl: './list-artist.component.html',
  styleUrls: ['./list-artist.component.css'],
  animations:[
    trigger('transition',[
      transition("* => *",[
          query(":leave",[
            style({ opacity: 1 }),
            stagger(50,animate(100, style({ opacity: 0})))
          ], { optional: true}),
          query(":enter",[
            style({ opacity: 0, transform: 'translateY(50px)' }),
            stagger(100,animate(200, style({ opacity: 1, transform:'translateY(0)'})))
          ],{ optional: true})
      ])
    ]),
    trigger('alphabetcard',[
      transition(":enter",[
        query("li",[
          style({ opacity: 0 , transform:'translateY(-20px)',
          textShadow:'4px 18px 9px rgba(25,25,25,0.7)'
        }),
          stagger(20,[
            animate("0.2s 500ms", style({
              opacity: 1, transform:'translateY(0px)',
              textShadow:'none'
            }))
          ])
        ])
      ])
    ])
  ]
})
export class ListArtistComponent implements OnInit, OnDestroy {

  page: number=1;
  maxPage: number=1;
  pages: number[];
  size: number=12;
  list: Artist[];
  alphabets: string[]=[];
  startsWith: string;
  beginBirth: number=new Date(1800,1,1,0,0,0,0).getTime();
  baseUrl="/artist";
  bound: number=7;
  private subc: Subscription;
  isLoading: boolean=false;
  constructor(
    private actRoute: ActivatedRoute,
    private router: Router,
    private artistService: ArtistService,
    private loader: LoaderService,
    public lang: LanguageService
  ) { }

  ngOnInit() {
    this.alphabets=[
      this.lang.ui.all,
      "A","B","C","D","E","F","G","H",'I','J','K','L','M',
      'N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
    ];
    this.subc=this.actRoute.queryParams.subscribe(res=>{
      if (res['startsWith'] && res['startsWith'].length==1){
        this.startsWith=res['startsWith'];
      }
      if (res['page'])
      {
        this.page=+res['page'];
        this.setPage(this.page);
      }
      else {
        this.router.navigate([this.baseUrl],{queryParams:{page: 1}});
        window.scrollTo(0,0);
      }
    })
  }

  setPage(p: number){
    if (this.page!=p)
    {
      this.router.navigate([this.baseUrl],{queryParams:{page: p}});
      return;
    }
    if (p<=0 || p>this.maxPage) return;
    this.isLoading=true;
    this.artistService.startWith(this.startsWith,p,this.size)
    .subscribe(res=>{
      this.isLoading=false;
      if (res==null || res.maxPage==0)
      {
        this.page=1;
        this.maxPage=1;
        this.list=[];
        this.pages=[];
        return;
      }
      this.page=res.page;
      if (this.page!=p) {
        this.setPage(this.page);
        return;
      }
      this.pages=[];
      this.maxPage=res.maxPage;
      for (var i=1; i<=this.maxPage; i++)
        this.pages.push(i);
      const divide=this.bound/2;
      const offset=this.page-1<divide?0:this.page-1-divide;
      this.pages=this.pages.splice(offset,this.bound);
      this.list=res.list;
    },err=>{
      this.isLoading=false;
      if (err.status==404)
        this.router.navigate(['/notfound']);
      else
        this.router.navigate(['/']);
    })
  }

  setAlphabet(str: string){
    if (str==this.startsWith || (str.length>1 && !this.startsWith)) return;
    if (str.length==1){
      this.startsWith=str;
      this.router.navigate([this.baseUrl],{queryParams:{ page:1,startsWith: this.startsWith }});
    }
    else {
      this.startsWith="";
      this.router.navigate([this.baseUrl],{queryParams:{ page:1 }});
    }
  }

  ngOnDestroy(){
    this.subc.unsubscribe();
  }

  get url(){
    return environment.MEDIA_URL;
  }

  isValidDate(date: Date){
    return new Date(date).getTime()>=this.beginBirth;
  }
}
