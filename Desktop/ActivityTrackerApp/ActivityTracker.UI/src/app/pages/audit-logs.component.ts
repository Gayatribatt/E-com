import { Component } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { DatePipe, NgFor, NgIf } from '@angular/common';
import { ApiService } from '../services/api.service';
import { AuditLog } from '../models';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf, NgFor, DatePipe],
  template: `
    <h2>Audit Logs</h2>
    <form class="filters" [formGroup]="filterForm" (ngSubmit)="search()">
      <input type="text" placeholder="UserId" formControlName="userId" />
      <input type="text" placeholder="Action (Create/Update/Delete)" formControlName="action" />
      <input type="text" placeholder="Entity Name" formControlName="entityName" />
      <input type="date" formControlName="startDateUtc" />
      <input type="date" formControlName="endDateUtc" />
      <button type="submit">Search</button>
      <button type="button" (click)="exportCsv()">Export CSV</button>
    </form>

    <table *ngIf="logs.length">
      <thead>
      <tr>
        <th>When</th><th>User</th><th>Action</th><th>Entity</th><th>Changed Fields</th>
      </tr>
      </thead>
      <tbody>
        <tr *ngFor="let log of logs">
          <td>{{ log.timestampUtc | date:'medium' }}</td>
          <td>{{ log.userId ?? '-' }}</td>
          <td>{{ log.action }}</td>
          <td>{{ log.entityName }} ({{ log.entityId }})</td>
          <td>
            <div *ngFor="let c of log.changedFields">
              {{ c.field }}: {{ c.oldValue }} -> {{ c.newValue }}
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  `,
  styles: [`
    .filters { display: grid; grid-template-columns: repeat(7, minmax(120px, 1fr)); gap: 8px; margin-bottom: 16px; }
    input { padding: 8px; border: 1px solid #ccc; border-radius: 6px; }
    button { border: 0; border-radius: 6px; padding: 8px 10px; cursor: pointer; }
    table { width: 100%; border-collapse: collapse; }
    th, td { border: 1px solid #ddd; padding: 8px; vertical-align: top; }
  `],
})
export class AuditLogsComponent {
  logs: AuditLog[] = [];
  readonly filterForm;

  constructor(private readonly fb: FormBuilder, private readonly api: ApiService) {
    this.filterForm = this.fb.group({
      userId: [''],
      action: [''],
      entityName: [''],
      startDateUtc: [''],
      endDateUtc: [''],
      pageNumber: ['1'],
      pageSize: ['20'],
    });
    this.search();
  }

  search(): void {
    const filter = this.filterForm.getRawValue() as Record<string, string>;
    this.api.getAuditLogs(filter).subscribe((res) => (this.logs = res.items));
  }

  exportCsv(): void {
    const filter = this.filterForm.getRawValue() as Record<string, string>;
    this.api.exportAuditCsv(filter).subscribe((blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `audit-logs-${Date.now()}.csv`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
