import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './dashboard/student-dashboard.component';
import { MyScoresComponent } from './my-scores/my-scores.component';
import { CourseRegistrationComponent } from './course-registration/course-registration.component';
import { ProfileStudentComponent } from './profile/profilestudent.component';
import { ScheduleComponent } from './schedule/schedule.component';
import { StudentAnnouncementsComponent } from './announcements/student-announcements.component';
const routes: Routes = [
  {
    path: '',
    component: StudentComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: StudentDashboardComponent },
      { path: 'my-scores', component: MyScoresComponent },
      { path: 'course-registration', component: CourseRegistrationComponent },
      { path: 'profile', component: ProfileStudentComponent },
      { path: 'schedule', component: ScheduleComponent },
      { path: 'announcements', component: StudentAnnouncementsComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentRoutingModule { }
