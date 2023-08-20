import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { LanguageService } from 'src/app/services/language.service';
import { ConfigService } from 'src/app/services/config.service';
import { observable } from 'mobx-angular';
import { SystemConfig } from 'src/app/models/systemConfig';
import { Router } from '@angular/router';
import { MaterializeToastService } from 'src/app/directives/MaterializeR/materialize-toast.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-system-config',
  templateUrl: './system-config.component.html',
  styleUrls: ['./system-config.component.css']
})
export class SystemConfigComponent implements OnInit {
  @observable param: SystemConfig;
  @observable oldParam: SystemConfig;
  @observable isLoading: boolean=false;

  constructor(public lang: LanguageService,
    private cfg: ConfigService,
    private router: Router
    ) { }

  ngOnInit() {
    this.cfg.get().subscribe(param=>{
      this.param=param;
      this.oldParam=JSON.parse(JSON.stringify(param));
    }, err=>{
      if (err.status == 401 || err.status==403 || err.status==404)
      {
        this.router.navigate(["/notfound"]);
      }
    })
  }

  save(){
    this.isLoading=true;
    this.cfg.save(this.param).subscribe(()=>{
      this.isLoading=false;
      this.oldParam=JSON.parse(JSON.stringify(this.param));
      MaterializeToastService.send(this.lang.ui.message_requestsuccess,"green rounded");
    },err=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,"red rounded");
    })
  }

  reset(){
    this.param=JSON.parse(JSON.stringify(this.oldParam));
  }
}
