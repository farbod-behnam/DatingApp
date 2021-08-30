import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})

export class AuthGuard implements CanActivate {

  constructor(private acountService: AccountService, private toastr: ToastrService, private router: Router) {}


  canActivate(): Observable<boolean> {
    return this.acountService.currentUser$.pipe(
      map(user => {
        if (user) return true;
        else {
          this.router.navigateByUrl('/home');
          this.toastr.error('You shall not pass');
          return false;
        }

      })
    )
  }

  
}
