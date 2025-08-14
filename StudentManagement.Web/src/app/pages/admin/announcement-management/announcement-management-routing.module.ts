import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AnnouncementManagementComponent } from './announcement-management.component';
import { AnnouncementsListComponent } from './announcements-list/announcements-list.component';
import { AnnouncementFormComponent } from './announcement-form/announcement-form.component';

const routes: Routes = [
  {
    path: '',
    component: AnnouncementManagementComponent,
    children: [
      { path: '', pathMatch: 'full', component: AnnouncementsListComponent },
      { path: 'create', component: AnnouncementFormComponent },
      { path: ':id/edit', component: AnnouncementFormComponent },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AnnouncementManagementRoutingModule {}
