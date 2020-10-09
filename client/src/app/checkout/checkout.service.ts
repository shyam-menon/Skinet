import { IOrder, IOrderToCreate } from './../shared/models/order';
import { HttpClient } from '@angular/common/http';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { IDeliveryMethod } from '../shared/models/deliveryMethod';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Create an order. Course item 246
  // tslint:disable-next-line: typedef
  createOrder(order: IOrderToCreate){
    return this.http.post(this.baseUrl + 'orders', order);
  }

  getDeliveryMethods(): Observable<IDeliveryMethod[]> {
    return this.http.get(this.baseUrl + 'orders/deliveryMethods').pipe(
      map((dm: IDeliveryMethod[]) => {
        return dm.sort((a, b) => b.price - a.price);
      })
    );
  }
}
