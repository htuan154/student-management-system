import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AdminComponent } from './admin.component';
import { DashboardComponent } from './dashboard/dashboard.component';


const routes: Routes = [
  {
    path: '',
    component: AdminComponent,
    children: [

      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },

      { path: 'dashboard', component: DashboardComponent },
      {
        path: 'users',
        loadChildren: () =>
          import('./user-management/user-management.module').then(
            (m) => m.UserManagementModule
          ),
      },
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
      {
        path: 'employees',
        loadChildren: () =>
          import('./employee-management/employee-management.module').then(
            (m) => m.EmployeeManagementModule
          ),
      },
      {
        path: 'roles',
        loadChildren: () =>
          import('./role-management/role-management.module').then(
            (m) => m.RoleManagementModule
          ),
      },
      {
        path: 'courses',
        loadChildren: () =>
          import('./course-management/course-management.module').then(
            (m) => m.CourseManagementModule
          ),
      },
      {
        path: 'classes',
        loadChildren: () =>
          import('./class-management/class-management.module').then(
            (m) => m.ClassManagementModule
          ),
      },

      {
        path: 'assignments',
        loadChildren: () =>
          import('./teacher-course-management/teacher-course-management.module').then(
            (m) => m.TeacherCourseManagementModule
          ),
      },

      {
        path: 'enrollments',
        loadChildren: () =>
          import('./enrollment-management/enrollment-management.module').then(
            (m) => m.EnrollmentManagementModule
          ),
      },
      {
        path: 'scores',
        loadChildren: () =>
          import('./score-management/score-management.module').then(
            (m) => m.ScoreManagementModule
          ),
      },
      {
        path: 'announcement-management',
        loadChildren: () =>
          import('./announcement-management/announcement-management.module').then(
            (m) => m.AnnouncementManagementModule
          ),
      },
      {
        path: 'semester-management',
        loadChildren: () =>
          import('./semester-management/semester-management.module').then(m => m.SemesterManagementModule)
      },
      {
        path: 'schedule-management',
        loadChildren: () =>
          import('./schedule-management/schedule-management.module').then(
            (m) => m.ScheduleManagementModule
          ),
      },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AdminRoutingModule { }
