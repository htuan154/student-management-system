// src/app/models/role.model.ts

import { User } from './user.model';

export interface Role {
  roleId: string;
  roleName: string;
  description?: string | null;
  users?: User[];
}
