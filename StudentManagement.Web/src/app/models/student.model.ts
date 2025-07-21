// src/app/models/student.model.ts

import { User } from './user.model';
import { Enrollment } from './enrollment.model';
import { Class } from './class.model';
export interface Student {
  studentId: string;
  fullName: string;
  email: string;
  classId: string;
  phoneNumber?: string | null;
  address?: string | null;
  dateOfBirth?: string | Date | null;
  class: Class;
  user?: User[];
  enrollments?: Enrollment[];
}
