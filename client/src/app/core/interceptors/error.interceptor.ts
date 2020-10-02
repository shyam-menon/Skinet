import { NavigationExtras, Router } from '@angular/router';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { Injectable } from '@angular/core';
import { catchError } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

// In order to use this HttpInterceptor, it needs to added as a provided in the app module
// The req represents an outgoing request and the next represents the response to the request
// When there is an error in the response, that can be caught in the interceptor
@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
    // Router injected into the constructor gives navigation capabilities so that when an
    // error occurs redirection can occur to the correct pages
    constructor(private router: Router, private toastr: ToastrService) {}

    // Use the RxJS operators to catch any errors that comes back in the HTTP response. The pipe allows
    // chaining the RxJS operator to catch the error and get hold of the error and deal with it.
    // If we have an error object, check the status (400, 404 etc)
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(error => {
                if (error) {
                    if (error.status === 400) {
                        if (error.error.errors) {
                            throw error.error;
                        } else {
                            this.toastr.error(error.error.message, error.error.statusCode);
                        }
                    }
                    if (error.status === 401) {
                        this.toastr.error(error.error.message, error.error.statusCode);
                    }
                    if (error.status === 404) {
                        this.router.navigateByUrl('/not-found');
                    }
                    if (error.status === 500) {
                        const navigationExtras: NavigationExtras = {state: {error: error.error}};
                        this.router.navigateByUrl('/server-error', navigationExtras);
                    }
                }
                return throwError(error);
            })
        );
    }
}
