import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { CourseManagementRoutingModule } from './course-management-routing.module';
import { CourseListComponent } from './course-list/course-list.component';
import { CourseFormComponent } from './course-form/course-form.component';

@NgModule({

  imports: [
    CommonModule,
    CourseManagementRoutingModule,
    ReactiveFormsModule,
    CourseListComponent,
    CourseFormComponent
  ]
})
export class CourseManagementModule { }
