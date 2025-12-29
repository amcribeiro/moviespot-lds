import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CinemaComponent } from './cinema.component';

describe('Cinema', () => {
  let component: CinemaComponent;
  let fixture: ComponentFixture<CinemaComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CinemaComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CinemaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
