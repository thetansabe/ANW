import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RankingManagerComponent } from './ranking-manager.component';

describe('RankingManagerComponent', () => {
  let component: RankingManagerComponent;
  let fixture: ComponentFixture<RankingManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RankingManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RankingManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
