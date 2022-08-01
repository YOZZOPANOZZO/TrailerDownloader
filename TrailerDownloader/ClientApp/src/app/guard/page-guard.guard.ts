import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { Config } from '../models/config';
import { ConfigService } from '../services/config.service';

@Injectable({
  providedIn: 'root'
})
export class PageGuardGuard implements CanActivate {

  static config: Config;

  constructor(private router: Router, private configService: ConfigService) {}

  canActivate(
    next: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {

      return this.configService.getConfig().toPromise().then(res => {
        if (res) {
          PageGuardGuard.config = res;
          return true;
        }
        else {
          console.log('No config file so redirecting to setup page.');
          this.router.navigate(['setup']);
          return false;
        }
      });
  }

}
