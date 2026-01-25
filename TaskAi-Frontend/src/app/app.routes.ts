import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { TaskListComponent } from './components/task-list/task-list';
import { TaskDetailComponent } from './components/task-detail/task-detail';
import { authGuard } from './guards/auth.guard';
import { AiDashboardComponent } from './components/ai-dashboard/ai-dashboard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: 'tasks', 
    component: TaskListComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'task/:id', 
    component: TaskDetailComponent, 
    canActivate: [authGuard] 
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'tasks/:id', component: TaskDetailComponent, canActivate: [authGuard] },
  { path: 'ai-insights', component: AiDashboardComponent },
  { path: '', redirectTo: '/tasks', pathMatch: 'full' }
];