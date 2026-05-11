import { Component } from '@angular/core';
import { DatePipe, NgFor, NgIf } from '@angular/common';
import { ApiService } from '../services/api.service';
import { ActivitySummary, RecentActivity } from '../models';
import { timeout } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, DatePipe],
  template: `
    <section class="hero">
      <div>
        <h2>Dashboard Summary</h2>
        <p>Live overview of audit and activity metrics.</p>
      </div>
      <div class="badge">Real-Time</div>
    </section>

    <p *ngIf="loading">Loading dashboard...</p>
    <p class="error" *ngIf="error">{{ error }}</p>
    <button class="retry" *ngIf="error" type="button" (click)="loadSummary()">Retry</button>

    <section class="cards" *ngIf="summary">
      <article><h3>Total Audit Logs</h3><p>{{ summary.totalAuditLogs }}</p><small>Entity changes</small></article>
      <article><h3>Total Activity Logs</h3><p>{{ summary.totalActivityLogs }}</p><small>User/API interactions</small></article>
      <article><h3>Failed Logins</h3><p>{{ summary.failedLoginAttempts }}</p><small>Security signal</small></article>
      <article><h3>Successful Logins</h3><p>{{ summary.successfulLogins }}</p><small>Successful auth events</small></article>
      <article><h3>Total Deletes</h3><p>{{ summary.totalDeletes }}</p><small>Delete operations tracked</small></article>
    </section>

    <section class="chart-panel" *ngIf="summary">
      <section class="chart">
        <h3>Activity Chart</h3>
        <svg viewBox="0 0 600 220" class="line-chart">
          <polyline [attr.points]="chartPoints" fill="none" stroke="#2563eb" stroke-width="4" />
          <polyline [attr.points]="chartPoints" fill="url(#lineFill)" stroke="none" [attr.opacity]="0.35" />
          <defs>
            <linearGradient id="lineFill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stop-color="#60a5fa"></stop>
              <stop offset="100%" stop-color="#ffffff"></stop>
            </linearGradient>
          </defs>
        </svg>
        <div class="row" *ngFor="let item of chartData">
          <label>{{ item.label }} ({{ item.value }})</label>
          <div class="bar-track">
            <div class="bar-fill" [style.width.%]="item.percent"></div>
          </div>
        </div>
      </section>
      <section class="donut">
        <div class="ring">
          <strong>{{ summary.totalActivityLogs }}</strong>
          <span>Total events</span>
        </div>
      </section>
    </section>

    <section class="activity-feed" *ngIf="recentActivities.length">
      <h3>Recent Activities</h3>
      <div class="activity-item" *ngFor="let activity of recentActivities">
        <div>
          <strong>{{ activity.activityType }}</strong>
          <p>{{ activity.description }}</p>
          <small>{{ activity.endpoint || 'N/A' }}</small>
        </div>
        <div class="meta">
          <span class="code">{{ activity.statusCode }}</span>
          <small>{{ activity.timestampUtc | date:'short' }}</small>
        </div>
      </div>
    </section>
  `,
  styles: [`
    .hero { display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; }
    .hero h2 { margin: 0; font-size: 24px; color: #111827; }
    .hero p { margin: 4px 0 0; color: #6b7280; font-size: 13px; }
    .badge { background: #dcfce7; color: #166534; padding: 6px 10px; border-radius: 999px; font-size: 12px; font-weight: 700; }
    .cards { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 12px; }
    article { border: 1px solid #e5e7eb; border-radius: 12px; padding: 16px; background: #fff; box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    h3 { margin: 0 0 8px; font-size: 13px; color: #6b7280; font-weight: 600; }
    p { margin: 0; font-size: 24px; font-weight: 800; color: #0f172a; }
    small { color: #64748b; font-size: 11px; }
    .error { color: #dc2626; font-size: 14px; margin-bottom: 12px; }
    .retry { margin-bottom: 12px; background: #0f766e; }
    .chart-panel { margin-top: 16px; display: grid; grid-template-columns: 1fr 260px; gap: 14px; }
    .chart { border: 1px solid #e5e7eb; background: linear-gradient(145deg, #ffffff, #f8fbff); border-radius: 12px; padding: 18px; box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .chart h3 { margin-bottom: 14px; color: #1e3a8a; }
    .line-chart { width: 100%; height: 180px; margin-bottom: 10px; background: #f8fafc; border-radius: 10px; }
    .row { margin-bottom: 10px; }
    label { display: block; font-size: 13px; margin-bottom: 4px; color: #374151; }
    .bar-track { height: 14px; background: #e5e7eb; border-radius: 999px; overflow: hidden; }
    .bar-fill { height: 100%; background: linear-gradient(90deg, #06b6d4, #2563eb, #22c55e); border-radius: 999px; min-width: 2px; }
    .donut { border: 1px solid #e5e7eb; background: #fff; border-radius: 12px; display: grid; place-items: center; box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .ring { width: 170px; height: 170px; border-radius: 50%; background: conic-gradient(#2563eb 0 70%, #22c55e 70% 100%); display: grid; place-items: center; position: relative; }
    .ring::after { content: ''; width: 120px; height: 120px; border-radius: 50%; background: #fff; position: absolute; }
    .ring strong, .ring span { position: relative; z-index: 1; }
    .ring strong { font-size: 22px; color: #0f172a; }
    .ring span { font-size: 11px; color: #6b7280; margin-top: -4px; }
    .activity-feed { margin-top: 16px; background: #fff; border: 1px solid #e5e7eb; border-radius: 12px; padding: 14px 16px; box-shadow: 0 8px 18px rgba(15, 23, 42, 0.06); }
    .activity-feed h3 { margin: 0 0 10px; color: #1f2937; }
    .activity-item { display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #f1f5f9; padding: 10px 0; }
    .activity-item:first-of-type { border-top: 0; }
    .activity-item p { font-size: 13px; margin: 2px 0; color: #334155; font-weight: 500; }
    .activity-item small { color: #64748b; font-size: 12px; }
    .meta { text-align: right; display: grid; gap: 4px; }
    .code { background: #e2e8f0; padding: 2px 8px; border-radius: 999px; font-size: 12px; color: #334155; }
  `],
})
export class DashboardComponent {
  summary?: ActivitySummary;
  loading = true;
  error = '';
  chartData: Array<{ label: string; value: number; percent: number }> = [];
  chartPoints = '';
  recentActivities: RecentActivity[] = [];

  constructor(private readonly api: ApiService) {
    this.loadSummary();
  }

  loadSummary(): void {
    this.loading = true;
    this.error = '';
    this.api.getDashboardSummary().pipe(timeout(30000)).subscribe({
      next: (s) => {
        this.summary = s;
        this.loading = false;
        this.chartData = this.buildChartData(s);
        this.chartPoints = this.buildChartPoints(this.chartData);
        this.loadRecentActivities();
      },
      error: (err: { status?: number; name?: string }) => {
        this.loading = false;
        this.error = err?.status === 403
          ? 'Dashboard is Admin-only. Please login with an Admin account.'
          : err?.name === 'TimeoutError'
            ? 'Dashboard request timed out. Please retry.'
            : 'Failed to load dashboard data.';
      },
    });
  }

  private buildChartData(summary: ActivitySummary): Array<{ label: string; value: number; percent: number }> {
    const source = [
      { label: 'Audit Logs', value: summary.totalAuditLogs },
      { label: 'Activity Logs', value: summary.totalActivityLogs },
      { label: 'Failed Logins', value: summary.failedLoginAttempts },
      { label: 'Successful Logins', value: summary.successfulLogins },
      { label: 'Deletes', value: summary.totalDeletes },
    ];
    const max = Math.max(...source.map((x) => x.value), 1);
    return source.map((x) => ({
      ...x,
      percent: Math.round((x.value / max) * 100),
    }));
  }

  private buildChartPoints(data: Array<{ value: number }>): string {
    const width = 600;
    const height = 180;
    const max = Math.max(...data.map((x) => x.value), 1);
    const step = width / Math.max(data.length - 1, 1);
    return data
      .map((point, index) => {
        const x = index * step;
        const y = height - Math.round((point.value / max) * 140) - 20;
        return `${x},${y}`;
      })
      .join(' ');
  }

  private loadRecentActivities(): void {
    this.api.getRecentActivities().pipe(timeout(10000)).subscribe({
      next: (items) => (this.recentActivities = items),
      error: () => (this.recentActivities = []),
    });
  }
}
