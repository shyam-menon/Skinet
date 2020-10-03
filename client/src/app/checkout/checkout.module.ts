import { CheckoutRoutingModule } from './checkout-routing.module';
import { RouterModule, Routes } from '@angular/router';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CheckoutComponent } from './checkout.component';



@NgModule({
  declarations: [CheckoutComponent],
  imports: [
    CommonModule,
    CheckoutRoutingModule
  ]
})
export class CheckoutModule { }
