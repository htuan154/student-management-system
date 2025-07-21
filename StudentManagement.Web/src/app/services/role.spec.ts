import { TestBed } from '@angular/core/testing';

import { Role } from './role.service';

describe('Role', () => {
  let service: Role;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Role);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
