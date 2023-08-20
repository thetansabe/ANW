import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditChatRoomComponent } from './edit-chat-room.component';

describe('EditChatRoomComponent', () => {
  let component: EditChatRoomComponent;
  let fixture: ComponentFixture<EditChatRoomComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditChatRoomComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditChatRoomComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
