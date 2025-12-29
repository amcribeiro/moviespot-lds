import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CinemaForm } from './cinema-form.component';

describe('CinemaForm', () => {
  let component: CinemaForm;
  let fixture: ComponentFixture<CinemaForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CinemaForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CinemaForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
