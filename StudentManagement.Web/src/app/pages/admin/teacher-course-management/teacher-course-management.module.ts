import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { TeacherCourseManagementRoutingModule } from './teacher-course-management-routing.module';
import { TeacherCourseListComponent } from './teacher-course-list/teacher-course-list.component';
import { TeacherCourseFormComponent } from './teacher-course-form/teacher-course-form.component';

@NgModule({
  
  imports: [
    CommonModule,
    TeacherCourseManagementRoutingModule,
    ReactiveFormsModule,
    TeacherCourseListComponent,
    TeacherCourseFormComponent
  ]
})
export class TeacherCourseManagementModule { }
