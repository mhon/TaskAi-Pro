import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  registerData = {
    username: '',
    password: '',
    email: ''
  };

  onRegister() {
    this.authService.register(this.registerData).subscribe({
      next: () => {
        alert('Registration successful! Please login.');
        this.router.navigate(['/login']);
      },
      error: (err: any) => { // Added ': any' to fix the type error
        console.error(err); // Using the variable to fix the 'never read' error
        alert('Registration failed. Username might be taken.');
      }
    });
  }

}