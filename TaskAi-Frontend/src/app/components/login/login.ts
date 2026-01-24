import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service'; // In v20, services are also just auth.ts
import { Router } from '@angular/router';
import { email } from '@angular/forms/signals';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html', // Note: login.component.html is likely just login.html now
  styleUrls: ['./login.css']    // Note: login.component.css is likely just login.css now
})
export class Login { // In v20, the default class name is often just 'Login'
  private authService = inject(AuthService);
  private router = inject(Router);

  loginData = {
    email: '',
    password: ''
  };

  onSubmit() {
    this.authService.login(this.loginData).subscribe({
      next: (response) => {
        console.log('Login success', response);
        this.router.navigate(['/tasks']); // Redirect to tasks after login
      },
      error: (err) => {
        alert('Login failed. Please check your credentials.');
      }
    });
  }
}