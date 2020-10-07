import { delay, finalize } from 'rxjs/operators';
import { BusyService } from './../services/busy.service';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
    constructor(private busyService: BusyService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Whitelist the async email validator. Course item 202
        if (!req.url.includes('emailexists')) {
            this.busyService.busy();
        }
        return next.handle(req).pipe(
            delay(500),
            finalize(() => {
                this.busyService.idle();
            })
        );
    }

}