import { Injectable, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { User } from '../models/user';
import { environment } from '../../environments/environment'
import {JwtHelperService} from '@auth0/angular-jwt';
import { map, catchError } from 'rxjs/operators';
import { throwError, BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';
import hmacSHA512 from 'crypto-js/hmac-sha512';
import { Permission } from '../models/permission';
import * as pako from 'pako';

const jwt=new JwtHelperService();
export enum LoginState{
    NOT_LOGGED_IN, LOGGED_IN
}
@Injectable({ providedIn:'root'})
export class UserService implements OnInit{
    
    profile: BehaviorSubject<User>=new BehaviorSubject<User>(null);
    private loginState: BehaviorSubject<LoginState>=new BehaviorSubject<LoginState>(LoginState.NOT_LOGGED_IN);
    private level: number;
    private token: string;
    private URL:string =environment.HOST+"/api/user";
    constructor(private http: HttpClient, private router: Router){
        this.token=localStorage.getItem('access_token');
        if (this.IsAuthenticated){
            try{
                this.decode();
                this.profile.next(JSON.parse(this.uncompress(localStorage.getItem("profile"))) as User);
                
                this.loginState.next(LoginState.LOGGED_IN);
            }
            catch (e)
            {
                //console.log(e);
                this.logout();
            }
        }
    }
    ngOnInit(){
        
    }

    getToken(){
        if (this.token)
            return this.token;
        return '';
    }

    get IsAuthenticated(){
        if (jwt.isTokenExpired(this.token) && this.loginState.getValue()!=LoginState.NOT_LOGGED_IN)
            this.loginState.next(LoginState.NOT_LOGGED_IN);
        return this.token && this.token.length>0 && this.token!=='undefined' && !jwt.isTokenExpired(this.token);
    }

    next(profile: User){
        localStorage.setItem("profile",this.compress(JSON.stringify(profile)));
        this.profile.next(profile);
    }

    SetAuthentication(result: any){
        this.token=result.access_token;
        this.loginState.next(LoginState.LOGGED_IN);
        if (!result['profile']) this.logout();
        const profile=result['profile'] as User;
        localStorage.setItem("access_token",this.token);
        localStorage.setItem("userid",result.userid);
        console.log("Cause!");
        console.log(JSON.stringify(profile));
        localStorage.setItem("profile",this.compress(JSON.stringify(profile)));
        console.log("Cause!");
        this.profile.next(profile);
        console.log("Cause!");
        this.decode();
    }


    register(user: User){
        var header= new HttpHeaders({ 'Content-Type':'application/json' });
        return this.http.post(this.URL+"/register", user , { headers: header})
        .pipe(catchError(this.errHandler));
    }

    update(user: User){
        var header= new HttpHeaders({ 'Content-Type':'application/json','Authorization':'Bearer '+this.token });
        return this.http.post(this.URL+"/update", {user}, {headers: header})
        .pipe(catchError(this.errHandler));
    }

    login(username: string, password: string, rememberMe: boolean=false){
        const message=username+":"+password;
        const encodedPass=hmacSHA512(btoa(message),environment.secret_key);
        //console.log(encodedPass);
        const content=btoa(username+":"+encodedPass);
        var header= new HttpHeaders({ 
            'Content-Type':'application/json;', 
            'Authorization':'Amnhac '+ content
        });
        return this.http.post(this.URL+"/login",rememberMe,{headers:header})
        .pipe(map( res=> JSON.parse(res.toString())),catchError(this.errHandler));
    }
    logout(){
        this.loginState.next(LoginState.NOT_LOGGED_IN);
        localStorage.removeItem("access_token");
        localStorage.removeItem("userid");
        localStorage.removeItem("profile");
        this.profile.next(null);
        this.token="";
        this.router.navigate(['/']);
    }

    getUserLevel(){
        return this.level;
    }
    getAuthorizedHeader(contentType:string='application/json'){
        if (this.IsAuthenticated)
            return new HttpHeaders({ 'Content-Type':contentType,'Authorization':'Bearer '+this.token });
        return null;
    }
    getAuthorizedHeaderNoContent(){
        if (this.IsAuthenticated)
            return new HttpHeaders({ 'Authorization':'Bearer '+this.token });
        return null;
    }

    decode(){
        let tok=jwt.decodeToken(this.token);
        this.level=+tok[environment.ClaimRole];
    }

    getPermissionInfo(): Observable<Permission>{
        if (this.IsAuthenticated){
            return this.http.post(environment.HOST+"/api/permission/my","",{headers: this.getAuthorizedHeader()})
            .pipe(
                catchError(err=>throwError(err)),
                map(res=>res as Permission)
            );
        }
        return null;
    }

    save(profile: User){
        if (this.IsAuthenticated)
        {
            return this.http.put(environment.HOST+"/api/user/save",profile,{headers: this.getAuthorizedHeader()})
            .pipe(
                catchError( err=>throwError(err))
            )
        }
        return null;
    }

    changePassword(oldpassword: string, newpassword: string){
        if (this.IsAuthenticated)
        {
            const oldpass=this.profile.getValue().Username+":"+oldpassword;
            const encodedPass=hmacSHA512(btoa(oldpass),environment.secret_key);
            const newpass=this.profile.getValue().Username+":"+oldpassword;
            const encodedPass2=hmacSHA512(btoa(newpass),environment.secret_key);
            return this.http.put(environment.HOST+"/api/user",{ oldpass: btoa(encodedPass), newpass: btoa(encodedPass2)},{headers: this.getAuthorizedHeader()})
            .pipe(
                catchError( err=>throwError(err))
            )
        }
        return null;
    }

    uploadAvatar(file: File){
        if (this.IsAuthenticated)
        {
            const form=new FormData();
            form.append("file",file);
            return this.http.post(environment.HOST+"/api/user/upload/avatar",form,{headers: this.getAuthorizedHeaderNoContent(), responseType: 'text'})
            .pipe(
                catchError( err=>throwError(err))
            )
        }
        return null;
    }
    
    uploadBackground(file: File){
        if (this.IsAuthenticated)
        {
            const form=new FormData();
            form.append("file",file);
            return this.http.post(environment.HOST+"/api/user/upload",form,{headers: this.getAuthorizedHeaderNoContent(), responseType: 'text'})
            .pipe(
                catchError( err=>throwError(err))
            )
        }
        return null;
    }

    getAbsoluteUrl(url: string){
        return url?environment.HOST+"/resources/"+url:'';
    }

    getTargetInfo(id: string){
            return this.http.post(environment.HOST+"/api/user/getprofile",'"'+id+'"',{headers: this.getAuthorizedHeader()})
            .pipe(
                catchError( err=>throwError(err)),
                map(res=>res as User)
            )
    }
    
    get myProfile(){
        return this.http.post(environment.HOST+"/api/user/profile","",{ headers: this.getAuthorizedHeader()})
        .pipe(
            catchError(e=>throwError(e)),
            map(res=> JSON.parse(res+'') as User)
        )
    }

    get LoginState(){
        return this.loginState.asObservable();
    }

    private errHandler(err: HttpErrorResponse){
        return throwError(err);
    }

    compress(str: string){
        var binary= pako.deflate(str, {to: 'string'});
        return btoa(binary);
    }
    uncompress(cmp: string){
        var src=atob(cmp);
        return pako.inflate(src, {to:'string'});
    }
}