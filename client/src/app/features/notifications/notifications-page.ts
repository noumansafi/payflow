import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationsApiService } from '../../core/api/notifications-api.service';
import { problemMessage } from '../../core/http/problem-details';
import { NotificationItem } from '../../core/models/activity.models';
import { ToastService } from '../../core/ui/toast.service';

@Component({
  selector: 'app-notifications-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './notifications-page.html',
})
export class NotificationsPage implements OnInit {
  private readonly api = inject(NotificationsApiService);
  private readonly toast = inject(ToastService);

  readonly items = signal<NotificationItem[]>([]);
  readonly loading = signal(true);
  readonly busy = signal(false);
  unreadOnly = false;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.api.list(1, 50, this.unreadOnly ? false : undefined).subscribe({
      next: (page) => {
        this.items.set(page.items);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error, 'Could not load alerts.'));
        this.loading.set(false);
      },
    });
  }

  toggleUnread(): void {
    this.unreadOnly = !this.unreadOnly;
    this.reload();
  }

  markRead(item: NotificationItem): void {
    if (item.isRead || this.busy()) {
      return;
    }

    this.busy.set(true);
    this.api.markRead(item.id).subscribe({
      next: () => {
        this.items.update((list) =>
          list.map((n) => (n.id === item.id ? { ...n, isRead: true } : n)),
        );
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error));
        this.busy.set(false);
      },
    });
  }

  markAll(): void {
    if (this.busy()) {
      return;
    }

    this.busy.set(true);
    this.api.markAllRead().subscribe({
      next: (result) => {
        this.toast.success(`Marked ${result.markedCount} as read`);
        this.reload();
        this.busy.set(false);
      },
      error: (error: unknown) => {
        this.toast.error(problemMessage(error));
        this.busy.set(false);
      },
    });
  }
}
