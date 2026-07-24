import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'dashboard-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <h2>Dashboard</h2>
    <div>
      <p>Welcome back</p>
    </div>
  `
})
export class DashboardComponent {
  private auth = inject(AuthService);
}
