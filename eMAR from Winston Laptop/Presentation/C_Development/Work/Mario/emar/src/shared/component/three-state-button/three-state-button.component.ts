import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'three-state-button',
  templateUrl: './three-state-button.component.html',
  styleUrls: ['./three-state-button.component.scss']
})
export class ThreeStateButtonComponent implements OnInit {

  // @Input() description: string
  @Output() action: EventEmitter<any> = new EventEmitter()
  @Input() options: string[]

  value: number = null
  previousValue: number = null
  constructor() { }

  ngOnInit(): void {

  }

  getValue() {
    // return option_group.getAttribute('data-value') || ''
    return this.value
  }

  prev_value_timeout: number = null

  setValue(prev_value, value) {
    this.value = value

    if (prev_value !== null) {
      // option_group.setAttribute('data-prev_value', prev_value)
      this.previousValue = prev_value

      clearTimeout(this.prev_value_timeout)

      // this.prev_value_timeout = setTimeout(() => option_group.removeAttribute('data-prev_value'), 125)
      this.prev_value_timeout = setTimeout(() => this.previousValue = null, 125)
    } else {
      // option_group.removeAttribute('data-prev_value')
      this.previousValue = null
    }
  }

  onClick(option) {
    const prev_value = this.getValue()
    // const target = event.target
    const value = this.options.indexOf(option)

    this.setValue(prev_value, (value !== -1 && prev_value !== value) ? value : null )

    // console.log('3way onClick: action: ', this.action)

    this.action.emit(option)
  }
}
