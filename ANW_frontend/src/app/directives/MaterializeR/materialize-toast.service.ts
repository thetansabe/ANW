import { Injectable } from '@angular/core';

declare var M:any;
export const MaterializeToastService ={
  send: (msg: string, classes: string="rounded", duration: number=4000)=>{
    M.toast({html: msg, classes: classes, displayLength: duration});
  }
}
