import { TestBed } from '@angular/core/testing';

import { ProductsAdminService } from './products-admin.service';

describe('ProductsAdminService', () => {
  let service: ProductsAdminService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductsAdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
