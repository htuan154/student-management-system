import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import các component tương ứng
import { StudentComponent } from './student.component';
import { StudentDashboardComponent } from './dashboard/student-dashboard.component';
import { MyScoresComponent } from './my-scores/my-scores.component';
import { CourseRegistrationComponent } from './course-registration/course-registration.component';

const routes: Routes = [
  {
    path: '',
    component: StudentComponent, // Component layout chính
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: StudentDashboardComponent },
      { path: 'my-scores', component: MyScoresComponent },
      { path: 'course-registration', component: CourseRegistrationComponent },
      // Thêm các route khác cho sinh viên ở đây
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class StudentRoutingModule { }
