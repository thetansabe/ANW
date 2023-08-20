import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VideoPopoutComponent } from './video-popout.component';

describe('VideoPopoutComponent', () => {
  let component: VideoPopoutComponent;
  let fixture: ComponentFixture<VideoPopoutComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VideoPopoutComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VideoPopoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
