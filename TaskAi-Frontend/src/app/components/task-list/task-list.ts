import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router'; // Ensure RouterModule is here
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule], // Critical for [routerLink]
  templateUrl: './task-list.html',
  styleUrls: ['./task-list.css']
})
export class TaskListComponent implements OnInit {
  private fb = inject(FormBuilder);
  private taskService = inject(TaskService);
  private authService = inject(AuthService);
  private router = inject(Router);

  tasks: any[] = [];
  taskForm: FormGroup;
  showAddForm = false;

  constructor() {
    this.taskForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      dueDate: [''],
      priority: ['Medium', Validators.required], // Default
      category: [''],
      status: ['To Do', Validators.required]    // Default
    });
  }

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.taskService.getTasks().subscribe({
      next: (data) => this.tasks = data,
      error: (err) => console.error('Failed to load tasks', err)
    });
  }

  onAddTask() {
    if (this.taskForm.valid) {
      this.taskService.createTask(this.taskForm.value).subscribe({
        next: () => {
          this.loadTasks(); // Refresh list
          this.taskForm.reset();
          this.showAddForm = false;
        },
        error: (err) => alert('Error saving task. Check if API is running.')
      });
    }
  }

  logout() {
    this.authService.logout(); // Ensure your AuthService has a logout method
    this.router.navigate(['/login']);
  }
}