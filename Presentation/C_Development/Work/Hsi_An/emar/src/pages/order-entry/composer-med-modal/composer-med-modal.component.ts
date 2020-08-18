import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'composer-med-modal',
  templateUrl: './composer-med-modal.component.html',
  styleUrls: ['./composer-med-modal.component.scss'],
})
export class ComposerMedModalComponent implements OnInit {
  @Input() modalTitle: string;

  constructor() {}

  ngOnInit(): void {}
}
