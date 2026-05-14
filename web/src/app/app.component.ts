import { ChangeDetectionStrategy, Component } from '@angular/core';

import { NotificationHostComponent } from './core/notifications/notification-host.component';
import { TaskBoardComponent } from './features/tasks/ui/task-board.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [TaskBoardComponent, NotificationHostComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-task-board />
    <app-notification-host />
  `,
})
export class AppComponent {}
