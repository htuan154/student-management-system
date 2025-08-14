import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Import file routing và các component của module
import { StudentRoutingModule } from './student.routes';
import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './dashboard/student-dashboard.component';
import { MyScoresComponent } from './my-scores/my-scores.component';
import { CourseRegistrationComponent } from './course-registration/course-registration.component';
import { ProfileStudentComponent } from './profile/profilestudent.component';
import { ScheduleComponent } from './schedule/schedule.component';
@NgModule({

  imports: [
    CommonModule,
    RouterModule,
    StudentRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    StudentComponent,
    StudentDashboardComponent,
    MyScoresComponent,
    CourseRegistrationComponent,
    ProfileStudentComponent,
    ScheduleComponent
  ]
})
export class StudentModule { }
