import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// ✅ Import đúng cách cho standalone components
import { ScoreListComponent } from './score-list/score-list.component';
import { ScoreFormComponent } from './score-form/score-form.component';

const routes: Routes = [
  { path: '', component: ScoreListComponent },
  { path: 'add', component: ScoreFormComponent }, // ⬅ THÊM route này
  { path: 'create', component: ScoreFormComponent },
  { path: 'edit/:id', component: ScoreFormComponent },
  { path: 'create/:courseId/:teacherId', component: ScoreFormComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ScoreManagementRoutingModule { }
