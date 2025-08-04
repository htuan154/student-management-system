import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import các component tương ứng
import { TeacherComponent } from './teacher.component';
import { TeacherDashboardComponent } from './dashboard/teacher-dashboard.component';
import { MyCoursesComponent } from './my-courses/my-courses.component';
import { ClassListComponent } from './class-list/class-list.component';

const routes: Routes = [
  {
    path: '',
    component: TeacherComponent,
    children: [
      { path: 'dashboard', component: TeacherDashboardComponent },
      { path: 'my-courses', component: MyCoursesComponent },

      { path: 'class-list', component: ClassListComponent },

      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TeacherRoutingModule { }
