import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TaskListComponent } from './task-list'; // Change 'TaskList' to 'TaskListComponent'
import { ReactiveFormsModule } from '@angular/forms';

describe('TaskListComponent', () => { // Update the name here
  let component: TaskListComponent; // Update the name here
  let fixture: ComponentFixture<TaskListComponent>; // Update the name here

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      // We add these imports because the component needs them to run
      imports: [
        TaskListComponent, // Update the name here
        ReactiveFormsModule,
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TaskListComponent); // Update here
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});