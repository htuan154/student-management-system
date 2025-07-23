import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ScoreListComponent } from './score-list/score-list.component';
import { ScoreFormComponent } from './score-form/score-form.component';

const routes: Routes = [
  {
    path: '', // /admin/scores
    component: ScoreListComponent,
  },
  {
    path: 'new', // /admin/scores/new
    component: ScoreFormComponent,
  },
  {
    path: 'edit/:id', // /admin/scores/edit/1
    component: ScoreFormComponent,
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ScoreManagementRoutingModule { }
