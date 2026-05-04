import { TestBed } from '@angular/core/testing';

import { AdminCategoriesService} from './category.service';

describe('AdminCategoriesService', () => {
  let service: AdminCategoriesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminCategoriesService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
