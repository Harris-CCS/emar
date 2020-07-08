import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { SharedComponentsModule } from '../shared/shared.components.module';
import { OrderEntryModule } from '../pages/order-entry/order-entry.module';
//import { MedComposerComponent } from '../pages/med-composer/med-composer.component';
import { AppRoutingModule } from './app-routing.module';
import { PatientsDashboardComponent } from '../pages/patients-dashboard/patients-dashboard.component';
import { InteractionComponent } from '../pages/order-entry/interaction/interaction.component';

@NgModule({
  declarations: [
    AppComponent,
    //MedComposerComponent,
    PatientsDashboardComponent,
    InteractionComponent,
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
