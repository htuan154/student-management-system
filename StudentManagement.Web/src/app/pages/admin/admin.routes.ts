import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import tất cả các component sẽ được sử dụng trong routing
import { AdminComponent } from './admin.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { RoleManagementComponent } from './role-management/role-management.component';
import { CourseManagementComponent } from './course-management/course-management.component';
import { TeacherCourseManagementComponent } from './teacher-course-management/teacher-course-management.component';
import { ClassManagementComponent } from './class-management/class-management.component';
import { EnrollmentManagementComponent } from './enrollment-management/enrollment-management.component';
import { ScoreManagementComponent } from './score-management/score-management.component';

const routes: Routes = [
  {
    path: '',
    component: AdminComponent, // AdminComponent là layout chung
    children: [
      // Route mặc định, chuyển hướng đến dashboard
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      { path: 'dashboard', component: DashboardComponent },
      { path: 'users', component: UserManagementComponent },

      {
        path: 'students',
        loadChildren: () =>
          import('./student-management/student-management.module').then(
            (m) => m.StudentManagementModule
          ),
      },
      {
        path: 'teachers',
        loadChildren: () =>
          import('./teacher-management/teacher-management.module').then(
            (m) => m.TeacherManagementModule
          ),
      },

      { path: 'roles', component: RoleManagementComponent },
      { path: 'courses', component: CourseManagementComponent },
      { path: 'classes', component: ClassManagementComponent },
      { path: 'assignments', component: TeacherCourseManagementComponent },
      { path: 'enrollments', component: EnrollmentManagementComponent },
      { path: 'scores', component: ScoreManagementComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
