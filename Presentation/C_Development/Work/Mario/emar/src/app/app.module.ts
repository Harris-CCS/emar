import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { SharedComponentsModule } from '../shared/shared.components.module';
import { OrderEntryModule } from '../pages/order-entry/order-entry.module';
//import { MedComposerComponent } from '../pages/med-composer/med-composer.component';
import { AppRoutingModule } from './app-routing.module';
import { PatientsDashboardComponent } from '../pages/patients-dashboard/patients-dashboard.component';
import { environment } from 'src/environments/environment';

@NgModule({
  declarations: [
    AppComponent,
    //MedComposerComponent,
    PatientsDashboardComponent,
  ],
  imports: [
    BrowserModule,
    SharedComponentsModule,
    OrderEntryModule,
    AppRoutingModule,
    HttpClientModule,
  ],
  providers: [
    //{ provide: "BASE_API_URL", useValue: environment.apiUrl},
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
