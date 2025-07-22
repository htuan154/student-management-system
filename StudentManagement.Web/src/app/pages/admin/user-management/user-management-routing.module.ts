import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { UserListComponent } from './user-list/user-list.component';
import { UserFormComponent } from './user-form/user-form.component';

const routes: Routes = [
  {
    path: '', // /admin/users
    component: UserListComponent,
  },
  {
    path: 'new', // /admin/users/new
    component: UserFormComponent,
  },
  {
    path: 'edit/:id', // /admin/users/edit/U001
    component: UserFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserManagementRoutingModule { }
