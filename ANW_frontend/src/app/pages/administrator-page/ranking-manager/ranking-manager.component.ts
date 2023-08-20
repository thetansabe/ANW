import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { observable } from 'mobx-angular';
import { Song } from 'src/app/models/song';
import { SongRanked } from 'src/app/models/songRanked';
import { RankingService } from 'src/app/services/ranking.service';
import { RankingCategory } from 'src/app/models/rankingCategory';
import { LanguageService } from 'src/app/services/language.service';
import { ActivatedRoute, Router } from '@angular/router';
import { LoaderService } from 'src/app/services/loader.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-ranking-manager',
  templateUrl: './ranking-manager.component.html',
  styleUrls: ['./ranking-manager.component.css']
})
export class RankingManagerComponent implements OnInit {
  @observable rankList: SongRanked[];
  @observable category: RankingCategory[];
  @observable currentPage: number=1;
  @observable maxPage: number=1;
  @observable filter: string;
  @observable pages: number[]=[];

  private maxSize=12;
  constructor(private ranker: RankingService,
    public lang: LanguageService,
    public loader: LoaderService,
    private actRoute: ActivatedRoute,
    private router: Router
    ) { }

  ngOnInit() {
    this.actRoute.queryParams.subscribe(params=>{
      if (params['page'])
        this.currentPage=+params['page'];
      if (params['t'])
        this.filter=params['t'];
      this.load();
    })
    this.ranker.getCategory().subscribe((res)=>{
      this.category=res;
    });
  }

  calcPage(){
    this.pages=[];
    if (this.maxPage>0)
      for (var i=1; i<=this.maxPage; i++)
        this.pages.push(i);
  }

  load(){
    this.loader.isSubLoading=true;
    this.ranking.subscribe(res=>{
      this.loader.isSubLoading=false;
      this.rankList=res.list;
      this.maxPage=res.maxPage;
      this.calcPage();
    },err =>{
      this.loader.isSubLoading=false;
      if (err.status==404)
        this.router.navigate(['/notfound']);
    })
  }

  get ranking(){
    if (this.filter)
      return this.ranker.filter(this.filter,this.currentPage,this.maxSize);
    else return this.ranker.get(this.currentPage,this.maxSize);
  }
}
