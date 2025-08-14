import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Import file routing và các component của module
import { TeacherRoutingModule } from './teacher.routes';
import { TeacherComponent } from './teacher.component';
import { TeacherDashboardComponent } from './dashboard/teacher-dashboard.component';
import { MyCoursesComponent } from './my-courses/my-courses.component';
import { ClassListComponent } from './class-list/class-list.component';
import { TeacherAnnouncementsComponent } from './announcements/teacher-announcements.component';
@NgModule({

  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    ReactiveFormsModule,
    TeacherRoutingModule,
    TeacherComponent,
    TeacherDashboardComponent,
    MyCoursesComponent,
    ClassListComponent,
    TeacherAnnouncementsComponent
  ]
})
export class TeacherModule { }
