import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, Router } from '@angular/router';

import { Dashboard } from './dashboard';

describe('Dashboard', () => {
  let component: Dashboard;
  let fixture: ComponentFixture<Dashboard>;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    fixture = TestBed.createComponent(Dashboard);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  // ─── Creación ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(component).toBeTruthy();
  });

  // ─── shortcuts ──────────────────────────────────────────────────────────────

  it('should expose two shortcuts', () => {
    expect(component.shortcuts).toHaveLength(2);
  });

  it('should have a shortcut for Gestionar Productos', () => {
    const shortcut = component.shortcuts.find(s => s.route === '/admin/productos');
    expect(shortcut).toBeDefined();
    expect(shortcut?.label).toBe('Gestionar Productos');
    expect(shortcut?.icon).toBe('inventory_2');
    expect(shortcut?.color).toBe('blue');
  });

  it('should have a shortcut for Gestionar Categorías', () => {
    const shortcut = component.shortcuts.find(s => s.route === '/admin/categorias');
    expect(shortcut).toBeDefined();
    expect(shortcut?.label).toBe('Gestionar Categorías');
    expect(shortcut?.icon).toBe('category');
    expect(shortcut?.color).toBe('purple');
  });

  // ─── go() ───────────────────────────────────────────────────────────────────

  it('should navigate to the given route when go() is called', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.go('/admin/productos');

    expect(router.navigate).toHaveBeenCalledWith(['/admin/productos']);
  });

  it('should navigate to /admin/categorias when go() is called with that route', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    component.go('/admin/categorias');

    expect(router.navigate).toHaveBeenCalledWith(['/admin/categorias']);
  });

  it('should not navigate when go() has not been called', () => {
    jest.spyOn(router, 'navigate').mockResolvedValue(true);

    expect(router.navigate).not.toHaveBeenCalled();
  });
});