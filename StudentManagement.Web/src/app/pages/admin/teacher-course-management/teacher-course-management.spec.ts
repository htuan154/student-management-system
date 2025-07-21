import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TeacherCourseManagement } from './teacher-course-management.component';

describe('TeacherCourseManagement', () => {
  let component: TeacherCourseManagement;
  let fixture: ComponentFixture<TeacherCourseManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [TeacherCourseManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TeacherCourseManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
