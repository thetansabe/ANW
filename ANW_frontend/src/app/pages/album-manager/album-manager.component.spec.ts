import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AlbumManagerComponent } from './album-manager.component';

describe('AlbumManagerComponent', () => {
  let component: AlbumManagerComponent;
  let fixture: ComponentFixture<AlbumManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AlbumManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AlbumManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
