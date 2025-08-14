// src/app/models/course.model.ts

import { Enrollment } from './enrollment.model';
import { TeacherCourse } from './teacher-course.model';
import { AnnouncementDetail } from './AnnouncementDetail.model';

export interface Course {
  courseId: string;
  courseName: string;
  credits: number;
  department?: string | null;
  description?: string | null;
  isActive: boolean;

  // Navigation properties
  teacherCourses?: TeacherCourse[];
  enrollments?: Enrollment[];
  announcementDetails?: AnnouncementDetail[];
}
