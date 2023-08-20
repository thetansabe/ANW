import { Component, OnInit, Input } from '@angular/core';
import { LanguageService } from 'src/app/services/language.service';
import { transition, trigger, style, animate } from '@angular/animations';

@Component({
  selector: 'app-cookie-policy',
  templateUrl: './cookie-policy.component.html',
  styleUrls: ['./cookie-policy.component.css'],
  animations:[
    trigger("transition",[
      transition(":enter",
        [
          style({ opacity: 0 }),
          animate("0.6s 2000ms ease-in-out",style({ opacity: 1}))
        ]
      ),
      transition(":leave",[
        animate("0.3s ease-in-out", style({ opacity: 0, transform: 'translateX(-130%)'}))
      ])
    ])
  ]
})
export class CookiePolicyComponent implements OnInit {

  @Input() hidden: boolean=false;
  constructor(
    public lang: LanguageService
  ) { }

  ngOnInit() {
  }

  hide(){
    this.hidden=true;
  }
}
