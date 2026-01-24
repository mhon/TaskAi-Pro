import { Component, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskService } from '../../services/task.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-ai-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './ai-dashboard.html',
  styleUrl: './ai-dashboard.css',
})
export class AiDashboardComponent {
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);

  aiSummary: string = '';
  loadingSummary = false;
  extractedData: any = null;
  isDragging = false;

  showToast = false;
  toastMessage = '';

  generateSummary() {
    this.loadingSummary = true;
    this.taskService.getAiSummary().subscribe({
      next: (res: { summary: string }) => {
        this.aiSummary = res.summary;
        this.loadingSummary = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('AI Error:', err);
        this.loadingSummary = false;
        this.cdr.detectChanges();
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.taskService.extractTaskData(file).subscribe({
        next: (data: any) => {
          this.extractedData = data;
          this.cdr.detectChanges();
        },
        error: (err) => console.error('Upload error:', err)
      });
    }
  }


  createFromAi(item: any) {
    const newTask = {
        title: item.taskName,
        description: 'Extracted from document',
        dueDate: item.dueDate,
        priority: 'Medium' as const,  // Fix 1
        status: 'To Do' as const      // Fix 2: Matches '"To Do" | "In Progress" | "Completed"'
    };

    this.taskService.createTask(newTask).subscribe({
        next: () => {
        this.toastMessage = `Successfully created: ${item.taskName}`;
        this.showToast = true;
        this.cdr.detectChanges();
        
        setTimeout(() => {
            this.showToast = false;
            this.cdr.detectChanges();
        }, 3000);
        },
        error: (err) => console.error('Error creating task:', err)
    });
    }

}