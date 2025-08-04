import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, NavigationEnd, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { TeacherService } from '../../services/teacher.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-teacher',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './teacher.component.html',
  styleUrls: ['./teacher.component.scss']
})
export class TeacherComponent implements OnInit, OnDestroy {
  teacherName: string | null = null;
  isSidebarOpen = false;
  currentPage = 'Bảng điều khiển';
  private routerSubscription: Subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private teacherService: TeacherService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadTeacherInfo();

    // Lắng nghe sự kiện thay đổi route để cập nhật tiêu đề
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: any) => {
        this.updateCurrentPage(event.urlAfterRedirects || event.url);
      });
    // Cập nhật tiêu đề lần đầu
    this.updateCurrentPage(this.router.url);
  }

  ngOnDestroy(): void {
    this.routerSubscription.unsubscribe();
  }

  loadTeacherInfo(): void {
    // Lấy payload đã được giải mã từ token
    const tokenPayload = this.authService.getDecodedToken();

    if (!tokenPayload) {
      console.error("Không thể giải mã token xác thực.");
      this.teacherName = "Giáo viên";
      return;
    }

    console.log('Token payload:', tokenPayload); // Debug log

    // Thử các claim phổ biến cho user ID
    const userId = tokenPayload.sub ||
                   tokenPayload.userId ||
                   tokenPayload.id ||
                   tokenPayload.teacherId ||
                   tokenPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
                   tokenPayload.nameid;

    // Thử lấy tên trực tiếp từ token trước
    const nameFromToken = tokenPayload.name ||
                         tokenPayload.fullName ||
                         tokenPayload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                         tokenPayload.username;

    if (nameFromToken) {
      this.teacherName = nameFromToken;
      console.log('Lấy tên từ token:', nameFromToken);
      return;
    }

    // Nếu không có tên trong token, thử gọi API với userId
    if (userId) {
      console.log('Trying to load teacher info with userId:', userId);
      this.teacherService.getTeacherById(userId).subscribe({
        next: (teacher) => {
          this.teacherName = teacher.fullName;
          console.log('Loaded teacher name from API:', teacher.fullName);
        },
        error: (error) => {
          console.error("Lỗi khi tải thông tin giáo viên:", error);
          // Fallback: sử dụng username hoặc email từ token
          this.teacherName = tokenPayload.username ||
                           tokenPayload.email ||
                           "Giáo viên";
        }
      });
    } else {
      console.error("Không tìm thấy ID người dùng trong token xác thực.");
      console.log("Available claims in token:", Object.keys(tokenPayload));

      // Fallback: sử dụng thông tin có sẵn trong token
      this.teacherName = tokenPayload.username ||
                       tokenPayload.email ||
                       "Giáo viên";
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private updateCurrentPage(url: string): void {
    if (url.includes('/dashboard')) this.currentPage = 'Bảng điều khiển';
    else if (url.includes('/my-courses')) this.currentPage = 'Lớp học của tôi';
    else if (url.includes('/class-list')) this.currentPage = 'Danh sách lớp';
    else this.currentPage = 'Bảng điều khiển';
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event): void {
    const target = event.target as Window;
    if (target.innerWidth > 768) {
      this.isSidebarOpen = false;
    }
  }
}
