import { Component, DestroyRef, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from './core/auth.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html'
})
export class App {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  mobileMenuOpen = false;

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.mobileMenuOpen = false;
      });
  }

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  isLoginRoute(): boolean {
    return this.router.url.startsWith('/login');
  }

  showShell(): boolean {
    return this.isAuthenticated() && !this.isLoginRoute();
  }

  userEmail(): string {
    return this.authService.getSignedInEmail();
  }

  userRole(): string {
    return this.authService.getSignedInRole();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  signOut(): void {
    this.authService.logout();
    this.closeMobileMenu();
    void this.router.navigate(['/login']);
  }
}