import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ClassListComponent } from './class-list/class-list.component';
import { ClassFormComponent } from './class-form/class-form.component';

const routes: Routes = [
  {
    path: '', // /admin/classes
    component: ClassListComponent,
  },
  {
    path: 'new', // /admin/classes/new
    component: ClassFormComponent,
  },
  {
    path: 'edit/:id', // /admin/classes/edit/C001
    component: ClassFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ClassManagementRoutingModule { }
