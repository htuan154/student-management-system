import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms'; // Cần cho form

import { TeacherManagementRoutingModule } from './teacher-management-routing.module';
import { TeacherListComponent } from './teacher-list/teacher-list.component';
import { TeacherFormComponent } from './teacher-form/teacher-form.component';

@NgModule({

  imports: [
    CommonModule,
    TeacherManagementRoutingModule,
    ReactiveFormsModule,
    TeacherListComponent,
    TeacherFormComponent
  ]
})
export class TeacherManagementModule { }
