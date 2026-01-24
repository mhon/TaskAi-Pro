import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AiDashboardComponent } from './ai-dashboard';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TaskService } from '../../services/task.service';
import { of } from 'rxjs';

describe('AiDashboardComponent', () => {
  let component: AiDashboardComponent;
  let fixture: ComponentFixture<AiDashboardComponent>;
  let taskService: TaskService;
  let mockTaskService: any;

  beforeEach(async () => {

    mockTaskService = {
      getAiSummary: () => of({ summary: 'AI Test Summary' }),
      extractTaskData: () => of({ tasks: [] })
    };

    await TestBed.configureTestingModule({
      imports: [AiDashboardComponent, HttpClientTestingModule],
      providers: [
        { provide: TaskService, useValue: mockTaskService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AiDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update aiSummary when generateSummary is called', () => {
    component.generateSummary();
    expect(component.aiSummary).toBe('AI Test Summary');
  });
});