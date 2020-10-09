import { IBasketItem } from './../../models/basket';
import { Observable } from 'rxjs';
import { BasketService } from './../../../basket/basket.service';
import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { IBasket } from '../../models/basket';

@Component({
  selector: 'app-basket-summary',
  templateUrl: './basket-summary.component.html',
  styleUrls: ['./basket-summary.component.scss']
})

// Create a separate component for basket summary. Course item 238
export class BasketSummaryComponent implements OnInit {
  basket$: Observable<IBasket>;
  @Output() decrement: EventEmitter<IBasketItem> = new EventEmitter<IBasketItem>();
  @Output() increment: EventEmitter<IBasketItem> = new EventEmitter<IBasketItem>();
  @Output() remove: EventEmitter<IBasketItem> = new EventEmitter<IBasketItem>();
  @Input() isBasket = true;


  constructor(private basketService: BasketService) { }

  ngOnInit(): void {
    this.basket$ = this.basketService.basket$;
  }

  decrementItemQuantity(item: IBasketItem): void {
    this.decrement.emit(item);
  }

  incrementItemQuantity(item: IBasketItem): void {
    this.increment.emit(item);
  }

  removeBasketItem(item: IBasketItem): void {
    this.remove.emit(item);
  }

}
