import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteAlbumComponent } from './delete-album.component';

describe('DeleteAlbumComponent', () => {
  let component: DeleteAlbumComponent;
  let fixture: ComponentFixture<DeleteAlbumComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DeleteAlbumComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DeleteAlbumComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
