import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskFormComponent } from './task-form.component';
import { makeTask } from '../data/task.fixtures';

describe('TaskFormComponent', () => {
  let fixture: ComponentFixture<TaskFormComponent>;
  let component: TaskFormComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('disables submit until a title is provided', () => {
    expect(component.form.invalid).toBeTrue();
  });

  it('emits save with normalised values', () => {
    const emitted = spyOn(component.save, 'emit');

    component.form.patchValue({
      title: '  Plan campaign  ',
      description: '',
      priority: 'High',
      assignee: '  ',
    });
    component.submit();

    expect(emitted).toHaveBeenCalledOnceWith(
      jasmine.objectContaining({
        title: 'Plan campaign',
        description: null,
        priority: 'High',
        assignee: null,
      }),
    );
  });

  it('does not emit when the form is invalid', () => {
    const emitted = spyOn(component.save, 'emit');

    component.form.controls.title.setValue('');
    component.submit();

    expect(emitted).not.toHaveBeenCalled();
    expect(component.form.controls.title.touched).toBeTrue();
  });

  it('pre-fills the form when editing a task', () => {
    const task = makeTask({
      title: 'Existing',
      description: 'Some context',
      priority: 'Low',
      assignee: 'Sam',
    });

    component.task = task;
    component.ngOnChanges({
      task: { currentValue: task, previousValue: null, firstChange: false, isFirstChange: () => false },
    });

    expect(component.form.value).toEqual(
      jasmine.objectContaining({
        title: 'Existing',
        description: 'Some context',
        priority: 'Low',
        assignee: 'Sam',
      }),
    );
  });
});
