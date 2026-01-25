import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TaskService } from '../../services/task.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.css',
})
export class TaskDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);

  task: any = null;
  isEditing = false;

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.taskService.getTaskById(id).subscribe({
        next: (data: any) => {
          console.log('Assigning data to UI:', data);
          this.task = data;
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('API Error details:', err);
        }
      });
    }
  }

  onUpdate() {
  const taskPayload = {
    ...this.task,
    category: this.task.category || "" 
  };

  this.taskService.updateTask(this.task.id, taskPayload).subscribe({
      next: (updatedData) => {
        this.task = { ...taskPayload }; 
        this.isEditing = false;
        this.cdr.detectChanges(); 
        alert('Task updated successfully!');
      },
      error: (err) => {
        console.error('Update failed:', err);
        alert('Failed to save changes. Please check your connection.');
      }
    });
  }

  onDelete() {
    if (confirm('Permanently delete this task?')) {
      this.taskService.deleteTask(this.task.id).subscribe({
        next: () => this.router.navigate(['/tasks'])
      });
    }
  }
}
