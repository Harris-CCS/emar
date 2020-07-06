import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { SharedComponentsModule } from '../shared/shared.components.module';
import { OrderEntryModule } from '../pages/order-entry/order-entry.module';
import { ComposerMedComponent } from '../pages/composer-med/composer-med.component';
import { AppRoutingModule } from './app-routing.module';
import { PatientsDashboardComponent } from '../pages/patients-dashboard/patients-dashboard.component';

@NgModule({
  declarations: [
    AppComponent,
    ComposerMedComponent,
    PatientsDashboardComponent,
  ],
  imports: [
    BrowserModule,
    SharedComponentsModule,
    OrderEntryModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
