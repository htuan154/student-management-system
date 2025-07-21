import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ScoreManagementComponent } from './score-management.component';

describe('ScoreManagementComponent', () => {
  let component: ScoreManagementComponent;
  let fixture: ComponentFixture<ScoreManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ScoreManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ScoreManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
