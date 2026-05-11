import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgFor, NgIf } from '@angular/common';
import { ApiService } from '../services/api.service';
import { TaskItem } from '../models';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [ReactiveFormsModule, NgFor, NgIf],
  template: `
    <section class="panel">
      <h2>Tasks</h2>
      <p>Create and track activities to generate audit trails.</p>
    </section>
    <form class="grid card" [formGroup]="form" (ngSubmit)="save()">
      <input type="text" placeholder="Title" formControlName="title" />
      <input type="text" placeholder="Description" formControlName="description" />
      <label><input type="checkbox" formControlName="isCompleted" /> Completed</label>
      <button type="submit">{{ editId ? 'Update' : 'Add' }}</button>
      <button type="button" *ngIf="editId" (click)="cancelEdit()">Cancel</button>
    </form>

    <table *ngIf="tasks.length" class="card">
      <thead>
        <tr><th>Id</th><th>Title</th><th>Description</th><th>Status</th><th>Actions</th></tr>
      </thead>
      <tbody>
        <tr *ngFor="let t of tasks">
          <td>{{ t.id }}</td>
          <td>{{ t.title }}</td>
          <td>{{ t.description }}</td>
          <td>{{ t.isCompleted ? 'Done' : 'Open' }}</td>
          <td>
            <button type="button" (click)="edit(t)">Edit</button>
            <button type="button" (click)="remove(t.id)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
  `,
  styles: [`
    .panel h2 { margin: 0; color: #111827; }
    .panel p { margin: 6px 0 14px; color: #6b7280; font-size: 13px; }
    .card { border: 1px solid #e5e7eb; border-radius: 12px; background: #fff; box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .grid { display: grid; grid-template-columns: 1fr 1fr auto auto auto; gap: 8px; margin-bottom: 16px; align-items: center; padding: 14px; }
    input[type=text] { padding: 9px; border: 1px solid #d1d5db; border-radius: 8px; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #e5e7eb; padding: 10px; text-align: left; }
    th { background: #f8fafc; color: #334155; }
    button { border: 0; border-radius: 8px; padding: 8px 10px; margin-right: 6px; cursor: pointer; background: #2563eb; color: #fff; }
  `],
})
export class TasksComponent implements OnInit {
  tasks: TaskItem[] = [];
  editId: number | null = null;
  readonly form;

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      isCompleted: [false],
    });
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getTasks().subscribe((tasks) => (this.tasks = tasks));
  }

  save(): void {
    if (this.form.invalid) return;
    const payload = this.form.getRawValue() as { title: string; description: string; isCompleted: boolean };
    if (this.editId) {
      this.api.updateTask(this.editId, payload).subscribe(() => {
        this.cancelEdit();
        this.load();
      });
      return;
    }
    this.api.createTask(payload).subscribe(() => {
      this.form.reset({ title: '', description: '', isCompleted: false });
      this.load();
    });
  }

  edit(item: TaskItem): void {
    this.editId = item.id;
    this.form.patchValue(item);
  }

  cancelEdit(): void {
    this.editId = null;
    this.form.reset({ title: '', description: '', isCompleted: false });
  }

  remove(id: number): void {
    this.api.deleteTask(id).subscribe(() => this.load());
  }
}
