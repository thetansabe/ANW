import { Component, OnInit, OnDestroy } from '@angular/core';
import { LanguageService } from '../../../services/language.service';
import { ArtistService } from '../../../services/artist.service';
import { Artist } from '../../../models/artist';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LoaderService } from 'src/app/services/loader.service';

@Component({
  selector: 'app-artist-details',
  templateUrl: './artist-details.component.html',
  styleUrls: ['./artist-details.component.css']
})
export class ArtistDetailsComponent implements OnInit, OnDestroy{

  artist: Artist;
  constructor(
    public lang:LanguageService,
    private loader: LoaderService,
    private artistService: ArtistService,
    private actRoute: ActivatedRoute,
    private router: Router
  ) { }

  private subc: Subscription;
  ngOnInit() {
    this.loader.isSubLoading=true;
    this.subc=this.actRoute.params.subscribe(params=>
      {
        if (params['id']){
          window.scrollTo(0,0);
          this.artistService.getById(params['id']).subscribe(res=>{
            this.artist=res;
            this.loader.isSubLoading=false;
          },err=>{
            if (err.status==404)
              this.router.navigate(['/notfound']);
          })
        }
        else 
          this.router.navigate(['/notfound']);
      })
  }

  get url(){
    return environment.MEDIA_URL;
  }

  ngOnDestroy(){
    this.subc.unsubscribe();
  }
}
