// src/app/models/teacher-course.model.ts

import { Teacher } from './teacher.model';
import { Course } from './course.model';

export interface TeacherCourse {
  teacherCourseId: number;
  teacherId: string;
  courseId: string;
  isActive: boolean;

  semester?: string | null;
  year?: number | null;

  teacher: Teacher;
  course: Course;
}
