import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { Router } from '@angular/router';
import { User } from '../../models/user';
import { LanguageService } from '../../services/language.service';
import { Permission } from '../../models/permission';
import { MaterializeToastService } from '../../directives/MaterializeR/materialize-toast.service';
import { SimpleTransition } from '../../components/transitions/simple.transition';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css'],
  animations: [
    SimpleTransition
  ]
})
export class AccountComponent implements OnInit {

  perm: Permission;
  info: User;
  isLoading=false;
  editEnable=false;
  constructor(
    private userService: UserService, 
    private router: Router,
    public lang: LanguageService
  ) { }

  ngOnInit() {
    if (!this.userService.IsAuthenticated){
      this.router.navigate(['/login'], {queryParams:{returnRoute:"/account"}});
      return;
    }
    this.userService.myProfile.subscribe(val=>this.info=val);
    this.userService.getPermissionInfo().subscribe(res=>{
      this.perm=res;
    }, err=>{
      if (err.status==403)
        this.router.navigate(['/login'], {queryParams:{returnRoute:"/account"}});
    })
  }

  submit(){
    this.isLoading=true;
    this.userService.save(this.info).subscribe(res=>{
      this.isLoading=false;
      this.userService.next(this.info);
      MaterializeToastService.send(this.lang.ui.message_saved,"green rounded");
    },err=>{
      this.isLoading=false;
      if (err.status==403)
        this.router.navigate(['/login'],{ queryParams: {returnRoute:'/account'}});
      else if (err.status==401)
      {
        MaterializeToastService.send(this.lang.ui.message_forbid,'red rounded');
      }
      else MaterializeToastService.send(this.lang.ui.message_requestfail,'red rounded');
    })
  }

  upload(e){
    if (this.isLoading) return;
    const file=e.target.files[0];
    this.isLoading=true;
    this.userService.uploadAvatar(file).subscribe(res=>{
      this.isLoading=false;
      this.info.AvatarImg=res.slice(1,res.length-1);
      e.target.value="";
      this.userService.next(this.info);
      MaterializeToastService.send(this.lang.ui.message_requestsuccess,'green rounded');
    },err=>{
      this.isLoading=false;
      MaterializeToastService.send(this.lang.ui.message_requestfail,'red rounded');
    })
  }

  enableEdit(){
    this.editEnable=true;
  }

  get getUrl(){
    return this.userService.getAbsoluteUrl(this.info.AvatarImg);
  }
}
