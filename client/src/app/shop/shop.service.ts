import { ShopParams } from './../shared/models/shopParams';
import { IType } from './../shared/models/productType';
import { IBrand } from './../shared/models/brands';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { IPagination, Pagination } from '../shared/models/pagination';
import { map } from 'rxjs/operators';
import { IProduct } from '../shared/models/product';

@Injectable({
  providedIn: 'root'
})
export class ShopService {
  baseUrl = 'https://localhost:5001/api/';
  products: IProduct[] = [];
  brands: IBrand[] = [];
  types: IType[] = [];
  pagination = new Pagination();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) { }

  getproducts(useCache: boolean): Observable<IPagination> {
    if (useCache === false) {
      this.products = [];
    }

    // client cache. Course item 285
    if (this.products.length > 0 && useCache === true) {
      const pagesReceived = Math.ceil(this.products.length / this.shopParams.pageSize);

      if (this.shopParams.pageNumber <= pagesReceived) {
        this.pagination.data =
              this.products.slice((this.shopParams.pageNumber - 1) *  this.shopParams.pageSize,
              this.shopParams.pageNumber * this.shopParams.pageSize);

        return of(this.pagination);
      }
    }

    let params = new HttpParams();

    if (this.shopParams.brandId !== 0) {
      params = params.append('brandId', this.shopParams.brandId.toString());
    }

    if (this.shopParams.typeId !== 0) {
      params = params.append('typeId', this.shopParams.typeId.toString());
    }

    if (this.shopParams.search) {
      params = params.append('search', this.shopParams.search);
    }

    params = params.append('sort', this.shopParams.sort);
    params = params.append('pageIndex', this.shopParams.pageNumber.toString());
    params = params.append('pageIndex', this.shopParams.pageSize.toString());

    return this.http.get<IPagination>(this.baseUrl + 'products', {observe: 'response', params})
      // use RxJS to project the response body to IPagination
      .pipe(
        map(response => {
          // client side caching. Course item 283 and 284
          // Append new results for products from API to existing response
          this.products = [...this.products, ...response.body.data];
          this.pagination = response.body;
          return this.pagination;
        })
      );
  }

  setShopParams(params: ShopParams): void {
    this.shopParams = params;
  }

  getShopParams(): ShopParams{
    return this.shopParams;
  }

  getProduct(id: number): Observable<IProduct> {
    const product = this.products.find(p => p.id === id);
    // client side caching. Course item 283
    if (product) {
      return of(product);
    }

    return this.http.get<IProduct>(this.baseUrl + 'products/' + id);
  }

  getBrands(): Observable<IBrand[]> {
    // client side caching. Course item 283
    if (this.brands.length > 0) {
      return of(this.brands);
    }
    return this.http.get<IBrand[]>(this.baseUrl + 'products/brands').pipe(
      map(response => {
        this.brands = response;
        return response;
      })
    );
  }

  getTypes(): Observable<IType[]> {
    // client side caching. Course item 283
    if (this.types.length > 0) {
      return of(this.types);
    }
    return this.http.get<IType[]>(this.baseUrl + 'products/types').pipe(
      map(response => {
        this.types = response;
        return response;
      })
    );
  }
}
