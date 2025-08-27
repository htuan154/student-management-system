import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({

  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']

})
export class AdminComponent implements OnInit, OnDestroy {
  isSidebarOpen = false;
  currentPage = 'Dashboard';
  private routerSubscription: Subscription = new Subscription();

  // Tiêm AuthService vào constructor
  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateCurrentPage(event.urlAfterRedirects || event.url);
      });
    this.updateCurrentPage(this.router.url);
  }

  ngOnDestroy(): void {
    this.routerSubscription.unsubscribe();
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  // Sửa lại hàm logout để gọi service
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  // Cập nhật đầy đủ các trang
  private updateCurrentPage(url: string): void {
    if (url.includes('/dashboard')) this.currentPage = 'Dashboard';
    else if (url.includes('/users')) this.currentPage = 'Quản lý Người dùng';
    else if (url.includes('/students')) this.currentPage = 'Quản lý Sinh viên';
    else if (url.includes('/teachers')) this.currentPage = 'Quản lý Giáo viên';
    else if (url.includes('/employees')) this.currentPage = 'Quản lý Nhân viên';
    else if (url.includes('/roles')) this.currentPage = 'Quản lý Vai trò';
    else if (url.includes('/courses')) this.currentPage = 'Quản lý Môn học';
    else if (url.includes('/classes')) this.currentPage = 'Quản lý Lớp học';
    else if (url.includes('/assignments')) this.currentPage = 'Quản lý Phân công';
    else if (url.includes('/enrollments')) this.currentPage = 'Quản lý Đăng ký';
    else if (url.includes('/scores')) this.currentPage = 'Quản lý Điểm';
    else if (url.includes('/announcement-management')) this.currentPage = 'Quản lý Thông báo';
    else if (url.includes('/semester-management')) this.currentPage = 'Quản lý Học kỳ';
    else if (url.includes('/schedule-management')) this.currentPage = 'Quản lý Lịch học';
    else this.currentPage = 'Dashboard';
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    const target = event.target as Window;
    if (target.innerWidth > 768) {
      this.isSidebarOpen = false;
    }
  }

}
