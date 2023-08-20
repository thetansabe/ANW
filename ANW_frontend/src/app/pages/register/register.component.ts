import { Component, OnInit, ViewChild, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';
import { User } from '../../models/user';
import { LanguageService } from '../../services/language.service';
import { debounceTime} from 'rxjs/operators';
import { trigger, transition, query, style, stagger, animate} from '@angular/animations'
import { MaterializeModalDirective } from '../../directives/MaterializeR/materialize-modal.directive';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { observable } from 'mobx-angular';

declare var M:any;
@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  animations:[
    trigger('transition',[
      //========== Begin
      transition(':enter',[
        // Begin transition
          query("div.col",[
            style({ opacity:0, transform:'scaleX(0.2)', backgroundColor:'rgba(0,0,0,0.6)'})
          ]),
          query('div.col', stagger(100,[
            //Animate
            animate('0.3s ease-in-out',style({ opacity:1, transform:'scaleX(1)', backgroundColor:'transparent'}))
            // End animate
          ]))
        // End of transition
      ])
      //========== End of trigger
    ])
  ]
})
export class RegisterComponent implements OnInit, OnDestroy {
  @observable reg=new User();
  @observable pass1="";
  @observable pass2="";
  @observable requesting=false;
  @observable register_form: FormGroup;
  @observable repeatInvalid=false;
  @observable registerValid=false;
  @observable emailConflicted=false;
  @observable userConflicted=false;
  @ViewChild(MaterializeModalDirective) private modal: MaterializeModalDirective;
  constructor(public lang: LanguageService,private builder: FormBuilder, private router: Router,
    private userService: UserService
  ) { }

  ngOnInit() {
    this.initForm();
  }

  register(ev){
    //ev.preventDefault();
    const pass=this.register_form.get('password').value;
    const repeatpass=this.register_form.get('repeatpassword').value;
    if (pass!=repeatpass)
    {
      return;
    }
    this.requesting=true;
    let u: User= new User();
    u.Username=this.register_form.get('username').value;
    u.Password=pass;
    u.Email=this.register_form.get('email').value;
    u.FullName=this.register_form.get('fullname').value;
    u.Phone=this.register_form.get('phone').value;
    u.Idcard=this.register_form.get('idcard').value;
    this.userService.register(u).subscribe(val=>{
      this.requesting=false;
      MaterializeToastService.send(this.lang.ui.registeredsuccess,"rounded green");
      this.router.navigate(["/"]);
      this.userConflicted=!true;
      this.emailConflicted=!true;
    },err=>{
      this.requesting=false;
      if (err.status==400){
        const val=JSON.parse(err.error) as Object;
        if (val['userCount']){
          MaterializeToastService.send(this.lang.ui.usernameconflicted,"rounded red");
          this.userConflicted=true;
        }
        if (val['emailCount']){
          MaterializeToastService.send(this.lang.ui.emailconflicted,"rounded red");
          this.emailConflicted=true;
        }
      }
      //MaterializeToastService.send(this.lang.ui,"rounded green");
    })
  }

  initForm(){
    this.register_form=this.builder.group({
      username:['', [Validators.minLength(4), Validators.pattern('^[a-zA-Z][0-9a-zA-Z]+$'), Validators.required]],
      password:['', Validators.minLength(4)],
      repeatpassword:['', Validators.minLength(4)],
      email:['', [Validators.required, Validators.pattern("^[a-zA-Z][0-9a-zA-Z.]+@[0-9a-zA-Z.]+.[0-9a-zA-Z]$")]],
      fullname:'',
      phone:['', [Validators.pattern('^0[0-9]+$'), Validators.minLength(8)]],
      idcard:'',
      agreement:''
    });

    this.register_form.controls['username'].valueChanges
    .pipe( debounceTime(500)).subscribe(()=> this.registerValidate());
    this.register_form.controls['password'].valueChanges
    .pipe( debounceTime(400)).subscribe(()=> this.passwordValidate());
    this.register_form.controls['repeatpassword'].valueChanges
    .pipe( debounceTime(400)).subscribe(()=> this.passwordValidate());
    this.register_form.controls['agreement'].valueChanges
    .pipe( debounceTime(100)).subscribe(()=> this.registerValidate());
  }

  passwordValidate(){
    let pass1=this.register_form.get('password').value;
    let pass2=this.register_form.get('repeatpassword').value;
    this.repeatInvalid=pass1!=pass2 && this.passwordLengthValid();
    this.registerValidate();
  }
  passwordLengthValid(){
    let pass1=this.register_form.get('password').value;
    let pass2=this.register_form.get('repeatpassword').value;
    return pass1.length>0 && pass2.length>0;
  }

  registerValidate(){
    let agreement=this.register_form.get('agreement').value;
    let username=this.register_form.get('username').value;
    this.registerValid= !this.repeatInvalid && agreement && this.passwordLengthValid() && this.register_form.valid;
  }
  ngOnDestroy(): void {
    //Called once, before the instance is destroyed.
    //Add 'implements OnDestroy' to the class.
    this.modal.destroy();
  }

  gotoHomepage(){
    this.modal.close();
    this.router.navigate(['/']);
  }
}
