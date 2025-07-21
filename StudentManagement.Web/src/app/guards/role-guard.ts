import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // Lấy role yêu cầu từ dữ liệu của route (sẽ định nghĩa ở bước 4)
    const expectedRole = route.data['expectedRole'];
    const userRole = this.authService.getUserRole();

    if (this.authService.isAuthenticated() && userRole === expectedRole) {
      return true; // Cho phép truy cập nếu đúng role
    } else {
      // Nếu không đúng role, có thể chuyển hướng về trang lỗi hoặc trang login
      this.router.navigate(['/login']);
      return false;
    }
  }
}
