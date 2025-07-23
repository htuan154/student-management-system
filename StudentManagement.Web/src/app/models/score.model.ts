// src/app/models/score.model.ts

import { Enrollment } from './enrollment.model';

export interface Score {
  scoreId: number;
  enrollmentId: number;
  processScore?: number | null;
  midtermScore?: number | null;
  finalScore?: number | null;
  totalScore?: number | null;
  isPassed?: boolean | null;
  enrollment?: Enrollment;
// Thông tin sinh viên
  studentId?: string | null;
  fullName?: string | null;
}
