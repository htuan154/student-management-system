import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { RoleListComponent } from './role-list/role-list.component';
import { RoleFormComponent } from './role-form/role-form.component';

const routes: Routes = [
  {
    path: '', // /admin/roles
    component: RoleListComponent,
  },
  {
    path: 'new', // /admin/roles/new
    component: RoleFormComponent,
  },
  {
    path: 'edit/:id', // /admin/roles/edit/R001
    component: RoleFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RoleManagementRoutingModule { }
