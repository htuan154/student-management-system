import { TestBed } from '@angular/core/testing';

import { TeacherCourse } from './teacher-course.service';

describe('TeacherCourse', () => {
  let service: TeacherCourse;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TeacherCourse);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
