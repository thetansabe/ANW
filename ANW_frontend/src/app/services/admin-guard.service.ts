import { Injectable } from '@angular/core';
import { CanActivate, Router, CanActivateChild, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { UserService } from './user.service';
import { GuardLevel } from '../models/constants/GuardLevel';

@Injectable({
  providedIn: 'root'
})
export class AdminGuardService implements CanActivate, CanActivateChild{

  constructor(private userService: UserService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot){
    let level=GuardLevel.MANAGER;
    if (route.data.guardLevel)
      level=route.data.guardLevel;
    if (this.userService.IsAuthenticated && this.userService.getUserLevel()>=level) return true;
    this.router.navigate(['/login'], {queryParams: { returnRoute: state.url }});
    return false;
  }
  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot){
    let level=GuardLevel.MANAGER;
    if (route.data.guardLevel)
      level=route.data.guardLevel;
    if (this.userService.IsAuthenticated && this.userService.getUserLevel()>=level) return true;
    this.router.navigate(['/login'], {queryParams: { returnRoute: state.url }});
    return false;
  }
}
