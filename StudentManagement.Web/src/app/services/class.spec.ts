import { TestBed } from '@angular/core/testing';

import { Class } from './class.service';

describe('Class', () => {
  let service: Class;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Class);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
