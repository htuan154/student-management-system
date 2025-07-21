import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import các component cần thiết
import { TeacherListComponent } from './teacher-list/teacher-list.component';
import { TeacherFormComponent } from './teacher-form/teacher-form.component';

const routes: Routes = [
  {
    path: '', // Đường dẫn: /admin/teachers
    component: TeacherListComponent,
  },
  {
    path: 'new', // Đường dẫn: /admin/teachers/new
    component: TeacherFormComponent,
  },
  {
    path: 'edit/:id', // Đường dẫn: /admin/teachers/edit/T001
    component: TeacherFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TeacherManagementRoutingModule { }
