import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskDetailComponent } from './task-detail';

describe('TaskDetailComponent', () => {
  let component: TaskDetailComponent;
  let fixture: ComponentFixture<TaskDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TaskDetailComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
