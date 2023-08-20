import { Injectable } from '@angular/core';

@Injectable({ providedIn:'root'})
export class LoaderService{
    public isLoading:boolean=false;
    public isSubLoading:boolean=false;
    constructor(){
    }
}