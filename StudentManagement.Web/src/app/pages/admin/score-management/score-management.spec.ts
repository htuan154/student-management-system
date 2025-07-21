import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScoreManagement } from './score-management.component';

describe('ScoreManagement', () => {
  let component: ScoreManagement;
  let fixture: ComponentFixture<ScoreManagement>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ScoreManagement]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ScoreManagement);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
