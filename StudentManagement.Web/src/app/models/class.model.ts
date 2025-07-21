// src/app/models/class.model.ts

import { Student } from './student.model';
export interface Class {
  classId: string;
  className: string;
  major: string;
  isActive: boolean;
  academicYear?: string | null;
  semester?: number | null;
  students?: Student[];
}
