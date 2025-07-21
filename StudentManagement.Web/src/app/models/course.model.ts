// src/app/models/course.model.ts

import { Enrollment } from './enrollment.model';
import { TeacherCourse } from './teacher-course.model';

export interface Course {
  courseId: string;
  courseName: string;
  credits: number;
  isActive: boolean;

  // Thuộc tính tùy chọn
  department?: string | null;
  description?: string | null;

  // Mảng các đối tượng quan hệ
  teacherCourses?: TeacherCourse[];
  enrollments?: Enrollment[];
}
