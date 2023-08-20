import { Injectable } from '@angular/core';
import { CanActivate, Router, CanActivateChild, RouterStateSnapshot, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuardService implements CanActivate, CanActivateChild{

  constructor(private userService: UserService, private router: Router) { }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot){
    if (this.userService.IsAuthenticated) return true;
    this.router.navigate(['/login'], {queryParams: { returnRoute: state.url }});
    return false;
  }
  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot){
    if (this.userService.IsAuthenticated) return true;
    this.router.navigate(['/login'], {queryParams: { returnRoute: state.url }});
    return false;
  }
}
