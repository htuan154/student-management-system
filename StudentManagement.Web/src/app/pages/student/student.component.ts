import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { Router, NavigationEnd ,RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { StudentService } from '../../services/student.service';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-student',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './student.component.html',
  styleUrls: ['./student.component.scss']
})
export class StudentComponent implements OnInit, OnDestroy {
  studentName: string | null = null;
  isSidebarOpen = false;
  currentPage = 'Bảng điều khiển';
  private routerSubscription: Subscription = new Subscription();

  constructor(
    private authService: AuthService,
    private studentService: StudentService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadStudentInfo();

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

  loadStudentInfo(): void {
    // Lấy payload đã được giải mã từ token
    const tokenPayload = this.authService.getDecodedToken();
    // Tên claim 'studentId' phải khớp với những gì backend trả về trong token
    const studentId = tokenPayload?.studentId;

    if (studentId) {
      this.studentService.getStudentById(studentId).subscribe(student => {
        this.studentName = student.fullName;
      });
    } else {
      console.error("Không tìm thấy mã sinh viên (studentId) trong token xác thực.");
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
    else if (url.includes('/my-scores')) this.currentPage = 'Xem điểm';
    else if (url.includes('/course-registration')) this.currentPage = 'Đăng ký học phần';
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
