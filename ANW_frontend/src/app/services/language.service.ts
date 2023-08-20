import { Injectable } from '@angular/core';
import { locale } from '../../environments/locale/locale';

@Injectable({ providedIn:'root'})
export class LanguageService{
    public ui=locale.language;
    constructor(){
    }
}