import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeacherManagement } from './teacher-management';

describe('TeacherManagement', () => {
  let component: TeacherManagement;
  let fixture: ComponentFixture<TeacherManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TeacherManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TeacherManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
