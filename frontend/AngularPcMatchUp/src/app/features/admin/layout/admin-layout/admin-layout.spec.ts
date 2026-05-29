import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

import { AdminLayout } from './admin-layout';

// Token con email en claim de Microsoft
const makeToken = (payload: object) =>
  'eyJhbGciOiJIUzI1NiJ9.' +
  btoa(JSON.stringify(payload)).replace(/=/g, '') +
  '.fake';

const TOKEN_WITH_MS_EMAIL = makeToken({
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'admin@test.com',
});

const TOKEN_WITH_GENERIC_EMAIL = makeToken({
  email: 'generic@test.com',
});

const TOKEN_WITHOUT_EMAIL = makeToken({
  role: 'Admin',
});

describe('AdminLayout', () => {
  let component: AdminLayout;
  let fixture: ComponentFixture<AdminLayout>;
  let router: Router;

  beforeEach(async () => {
    localStorage.clear();

    await TestBed.configureTestingModule({
      imports: [AdminLayout],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideAnimations(),
      ],
    }).compileComponents();

    router = TestBed.inject(Router);
    jest.spyOn(router, 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    localStorage.clear();
    jest.restoreAllMocks();
  });

  function createComponent() {
    fixture = TestBed.createComponent(AdminLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  // ─── Creación ────────────────────────────────────────────────────────────────

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should have 3 nav items', () => {
    createComponent();
    expect(component.navItems.length).toBe(3);
  });

  it('should start with sidebar expanded', () => {
    createComponent();
    expect(component.sidebarCollapsed()).toBe(false);
  });

  // ─── adminEmail (constructor) ─────────────────────────────────────────────────

  it('should set adminEmail from MS email claim', () => {
    localStorage.setItem('token', TOKEN_WITH_MS_EMAIL);
    createComponent();
    expect(component.adminEmail()).toBe('admin@test.com');
  });

  it('should set adminEmail from generic email claim', () => {
    localStorage.setItem('token', TOKEN_WITH_GENERIC_EMAIL);
    createComponent();
    expect(component.adminEmail()).toBe('generic@test.com');
  });

  it('should set adminEmail to "Admin" when token has no email', () => {
    localStorage.setItem('token', TOKEN_WITHOUT_EMAIL);
    createComponent();
    expect(component.adminEmail()).toBe('Admin');
  });

  it('should leave adminEmail empty when no token in localStorage', () => {
    createComponent();
    expect(component.adminEmail()).toBe('');
  });

  // ─── toggleSidebar ────────────────────────────────────────────────────────────

  it('should collapse sidebar when toggleSidebar is called', () => {
    createComponent();
    component.toggleSidebar();
    expect(component.sidebarCollapsed()).toBe(true);
  });

  it('should expand sidebar when toggleSidebar is called twice', () => {
    createComponent();
    component.toggleSidebar();
    component.toggleSidebar();
    expect(component.sidebarCollapsed()).toBe(false);
  });

  // ─── isActive ─────────────────────────────────────────────────────────────────

  it('should return true when current URL starts with the given route', () => {
    createComponent();
    jest.spyOn(router, 'url', 'get').mockReturnValue('/admin/dashboard');
    expect(component.isActive('/admin/dashboard')).toBe(true);
  });

  it('should return false when current URL does not match the route', () => {
    createComponent();
    jest.spyOn(router, 'url', 'get').mockReturnValue('/admin/productos');
    expect(component.isActive('/admin/dashboard')).toBe(false);
  });

  it('should return true for partial route match (startsWith)', () => {
    createComponent();
    jest.spyOn(router, 'url', 'get').mockReturnValue('/admin/dashboard/detalle');
    expect(component.isActive('/admin/dashboard')).toBe(true);
  });

  // ─── logout ───────────────────────────────────────────────────────────────────

  it('should remove token from localStorage on logout', () => {
    localStorage.setItem('token', TOKEN_WITH_MS_EMAIL);
    createComponent();
    component.logout();
    expect(localStorage.getItem('token')).toBeNull();
  });

  it('should navigate to home on logout', () => {
    createComponent();
    component.logout();
    expect(router.navigate).toHaveBeenCalledWith(['']);
  });

  // ─── goToCatalog ──────────────────────────────────────────────────────────────

  it('should navigate to /productos when goToCatalog is called', () => {
    createComponent();
    component.goToCatalog();
    expect(router.navigate).toHaveBeenCalledWith(['/productos']);
  });
});