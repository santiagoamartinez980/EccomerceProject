import { TestBed } from '@angular/core/testing';

import { ProductsAdminServiceTs } from './products-admin.service.ts';

describe('ProductsAdminServiceTs', () => {
  let service: ProductsAdminServiceTs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductsAdminServiceTs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
