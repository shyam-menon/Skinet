import { delay, finalize } from 'rxjs/operators';
import { BusyService } from './../services/busy.service';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
    constructor(private busyService: BusyService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

        // Turn off loading interceptor for POST of orders. Course item 272
        if (req.method === 'POST' && req.url.includes('orders')) {
            return next.handle(req);
        }

        // Turn off loading indicator when deleting. Course item 286
        if (req.method === 'DELETE') {
            return next.handle(req);
        }
        // Whitelist the async email validator. Course item 202
        if (req.url.includes('emailexists')) {
            return next.handle(req);
        }
        this.busyService.busy();
        return next.handle(req).pipe(
            // Delay introduced to simulate remote server
            delay(500),
            finalize(() => {
                this.busyService.idle();
            })
        );
    }

}