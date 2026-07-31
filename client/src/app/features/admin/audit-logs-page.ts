import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { AuditApiService } from '../../core/api/audit-api.service';
import { problemMessage } from '../../core/http/problem-details';
import { AuditLogItem } from '../../core/models/activity.models';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-audit-logs-page',
  imports: [DatePipe],
  templateUrl: './audit-logs-page.html',
})
export class AuditLogsPage implements OnInit {
  private readonly api = inject(AuditApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<AuditLogItem[]>([]);
  readonly loading = signal(true);

  ngOnInit(): void {
    this.api.list().subscribe({
      next: (page) => {
        this.items.set(page.items);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not load audit logs.'));
        this.loading.set(false);
      },
    });
  }
}
