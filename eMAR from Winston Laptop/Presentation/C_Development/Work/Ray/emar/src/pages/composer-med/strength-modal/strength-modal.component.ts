import { Component, OnDestroy, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Subject, Subscription } from 'rxjs';
import { ComposerSchedulerService } from 'src/services/composer-scheduler.service';
import { ModalService } from '../../../services/modal.service';

@Component({
    selector: 'strength-modal',
    templateUrl: './strength-modal.component.html',
    styleUrls: ['./strength-modal.component.scss']
})

export class StrengthModalComponent implements OnInit, OnDestroy {
    @Input() strengths: string[];
    modalSubscribe: Subscription = null;

    constructor(private modalService: ModalService,
        private composerSchedulerService: ComposerSchedulerService) {
    }

    ngOnInit(): void {
        this.modalSubscribe = this.modalService.modalOpening.subscribe( modal => {
            this.strengths = modal.data.strengths;
        });
    }

    changeStrength(i: number) {
        this.composerSchedulerService.selectPopupStrength(i);
        // close the popup without emitting. Only the cancel will emit to go back to med service
        this.modalService.close('strengthModal', false, true);
    }
    ngOnDestroy(): void {
        if (this.modalSubscribe !== null) this.modalSubscribe.unsubscribe();
    }
}