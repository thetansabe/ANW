import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterializeModalDirective } from '../materialize-modal.directive';
import { MaterializeTooltipDirective } from '../materialize-tooltip.directive';
import { MaterializeCollapsibleDirective } from '../materialize-collapsible.directive';
import { MaterializeSelectDirective } from '../materialize-select.directive';
import { MaterializeChipsDirective } from '../materialize-chips.directive';
import { MaterializeDropdownDirective } from '../materialize-dropdown.directive';
import { MaterializeTabsDirective } from '../materialize-tabs.directive';
import { MaterializeDatepickerDirective } from '../materialize-datepicker.directive';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [
    MaterializeModalDirective,
    MaterializeTooltipDirective,
    MaterializeCollapsibleDirective,
    MaterializeSelectDirective,
    MaterializeChipsDirective,
    MaterializeDropdownDirective,
    MaterializeTabsDirective,
    MaterializeDatepickerDirective
  ],
  exports:[
    MaterializeModalDirective,
    MaterializeTooltipDirective,
    MaterializeCollapsibleDirective,
    MaterializeSelectDirective,
    MaterializeChipsDirective,
    MaterializeDropdownDirective,
    MaterializeTabsDirective,
    MaterializeDatepickerDirective
  ]
})
export class MaterializeRFormsModule { }
