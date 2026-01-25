import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onRegister() {
    if (this.registerForm.valid) {
      this.errorMessage = '';
      this.authService.register(this.registerForm.value).subscribe({
        next: (response) => {
          alert('Registration successful!');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          if (typeof err.error === 'string') {
            this.errorMessage = err.error;
          } else {
            this.errorMessage = err.error?.message || 'An unexpected error occurred.';
          }
          this.cdr.detectChanges(); 
          console.log('Variable this.errorMessage is now:', this.errorMessage);
        }
      });
    }
  }
}