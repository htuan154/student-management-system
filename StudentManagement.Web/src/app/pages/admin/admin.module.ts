import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { AdminRoutingModule } from './admin.routes';
import { AdminComponent } from './admin.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { UserManagementComponent } from './user-management/user-management.component';
import { TeacherManagementComponent } from './teacher-management/teacher-management.component';
import { RoleManagementComponent } from './role-management/role-management.component';
import { CourseManagementComponent } from './course-management/course-management.component';
import { TeacherCourseManagementComponent } from './teacher-course-management/teacher-course-management.component';
import { ClassManagementComponent } from './class-management/class-management.component';
import { StudentManagementComponent } from './student-management/student-management.component';
import { EnrollmentManagementComponent } from './enrollment-management/enrollment-management.component';
import { ScoreManagementComponent } from './score-management/score-management.component';
import { EmployeeManagementComponent } from './employee-management/employee-management.component';

@NgModule({

  imports: [
    AdminComponent,
    DashboardComponent,
    UserManagementComponent,
    CommonModule,
    RouterModule,
    AdminRoutingModule,
    TeacherManagementComponent,
    RoleManagementComponent,
    CourseManagementComponent,
    TeacherCourseManagementComponent,
    ClassManagementComponent,
    StudentManagementComponent,
    EnrollmentManagementComponent,
    ScoreManagementComponent,
    EmployeeManagementComponent
  ],

})
export class AdminModule { }
