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

@NgModule({
  declarations: [
    AppComponent,
    PatientsDashboardComponent
  ],
  imports: [
    BrowserModule,
    SharedComponentsModule,
    OrderEntryModule,
    AppRoutingModule,
    HttpClientModule,
    ComposerMedSelfModule,
  ],
  providers: [
    //{ provide: "BASE_API_URL", useValue: environment.apiUrl},
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
