import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { SharedComponentsModule } from '../shared/shared.components.module';
import { OrderEntryModule } from '../pages/order-entry/order-entry.module';
import { AppRoutingModule } from './app-routing.module';
import { PatientsDashboardComponent } from '../pages/patients-dashboard/patients-dashboard.component';
import { environment } from 'src/environments/environment';
import { ComposerMedSelfModule } from '../pages/composer-med-self/composer-med-self.module';
import { ComposerSchedulerService } from '../services/composer-scheduler.service';
import { PatientDashboardModule } from '../pages/patient-dashboard/patient-dashboard.module';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [AppComponent, PatientsDashboardComponent],
  imports: [
    BrowserModule,
    SharedComponentsModule,
    OrderEntryModule,
    AppRoutingModule,
    HttpClientModule,
    ComposerMedSelfModule,
    CommonModule,
    PatientDashboardModule
  ],
  providers: [
    //{ provide: "BASE_API_URL", useValue: environment.apiUrl},
    ComposerSchedulerService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
