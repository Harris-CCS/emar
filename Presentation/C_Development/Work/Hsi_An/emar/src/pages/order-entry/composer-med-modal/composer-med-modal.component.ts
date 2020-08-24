import { Component, OnInit, Input } from '@angular/core';
import { ComposerSchedulerService } from '../../../services/composer-scheduler.service';
import { ModalService } from '../../../services/modal.service';

@Component({
  selector: 'composer-med-modal',
  templateUrl: './composer-med-modal.component.html',
  styleUrls: ['./composer-med-modal.component.scss'],
})
export class ComposerMedModalComponent implements OnInit {
  @Input() modalTitle: string;

  constructor(
    private modalService: ModalService,
    private composerSchedulerService: ComposerSchedulerService
  ) {}

  ngOnInit(): void {
    this.modalService.formClosed.subscribe(() => {
      if (this.modalService.formClosed.value === 'medComposer') {
        this.composerSchedulerService.resetForm();
      }
    });
  }
}
