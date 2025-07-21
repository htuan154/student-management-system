// src/app/models/employee.model.ts

import { User } from './user.model';

export interface Employee {
  employeeId: string;
  fullName: string;
  email: string;
  hireDate: string | Date;
  phoneNumber?: string | null;
  department?: string | null;
  position?: string | null;
  dateOfBirth?: string | Date | null;
  salary?: number | null;
  users?: User[];
}
