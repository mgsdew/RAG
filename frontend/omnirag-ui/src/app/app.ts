import { Component, inject } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule, CommonModule],
  templateUrl: './app.html',
  styleUrls: ['./app.scss']
})
export class App {
  private auth = inject(AuthService);
  private router = inject(Router);

  get username() {
    return this.auth.getUsername();
  }

  get rolesText() {
    return this.auth.getRolesText();
  }

  get isAuth() {
    return this.auth.isAuthenticated();
  }

  logout() {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
