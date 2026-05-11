import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  template: `
    <h2>Register User</h2>
    <form class="card" [formGroup]="form" (ngSubmit)="submit()">
      <input type="text" placeholder="Username" formControlName="username" />
      <input type="password" placeholder="Password" formControlName="password" />
      <select formControlName="role">
        <option value="User">User</option>
        <option value="Admin">Admin</option>
      </select>
      <button type="submit" [disabled]="form.invalid || saving">Create User</button>
      <p class="ok" *ngIf="message">{{ message }}</p>
      <p class="err" *ngIf="error">{{ error }}</p>
    </form>
  `,
  styles: [`
    .card { display: grid; gap: 8px; max-width: 420px; border: 1px solid #ddd; border-radius: 8px; padding: 16px; }
    input, select { border: 1px solid #ccc; border-radius: 6px; padding: 8px; }
    button { border: 0; border-radius: 6px; padding: 8px 10px; cursor: pointer; }
    .ok { color: #166534; }
    .err { color: #dc2626; }
  `],
})
export class UsersComponent {
  saving = false;
  message = '';
  error = '';
  readonly form;

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      role: ['User', Validators.required],
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.message = '';
    this.error = '';
    this.api.register(this.form.getRawValue() as { username: string; password: string; role: 'Admin' | 'User' }).subscribe({
      next: () => {
        this.saving = false;
        this.message = 'User created successfully.';
        this.form.reset({ username: '', password: '', role: 'User' });
      },
      error: (err) => {
        this.saving = false;
        this.error = err?.error?.error ?? 'Registration failed.';
      },
    });
  }
}
