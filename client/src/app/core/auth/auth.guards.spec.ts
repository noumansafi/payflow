import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './auth.guards';
import { AuthService } from './auth.service';

describe('auth guards', () => {
  let router: Router;

  function setup(auth: Partial<AuthService>): void {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    });
    router = TestBed.inject(Router);
  }

  describe('authGuard', () => {
    it('whenAuthenticated_allowsActivation', () => {
      setup({
        isAuthenticated: () => true,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
      expect(result).toBe(true);
    });

    it('whenAnonymous_redirectsToLogin', () => {
      setup({
        isAuthenticated: () => false,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
      expect(result instanceof UrlTree).toBe(true);
      expect(router.serializeUrl(result as UrlTree)).toBe('/login');
    });
  });

  describe('guestGuard', () => {
    it('whenAnonymous_allowsActivation', () => {
      setup({
        isAuthenticated: () => false,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
      expect(result).toBe(true);
    });

    it('whenAuthenticated_redirectsHome', () => {
      setup({
        isAuthenticated: () => true,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));
      expect(router.serializeUrl(result as UrlTree)).toBe('/');
    });
  });

  describe('adminGuard', () => {
    it('whenAdmin_allowsActivation', () => {
      setup({
        isAuthenticated: () => true,
        isAdmin: () => true,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
      expect(result).toBe(true);
    });

    it('whenUser_redirectsHome', () => {
      setup({
        isAuthenticated: () => true,
        isAdmin: () => false,
      } as AuthService);

      const result = TestBed.runInInjectionContext(() => adminGuard({} as never, {} as never));
      expect(router.serializeUrl(result as UrlTree)).toBe('/');
    });
  });
});
