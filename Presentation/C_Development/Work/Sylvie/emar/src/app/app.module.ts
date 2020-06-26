import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { SharedComponentsModule } from '../shared/shared.components.module';
import { OrderEntryModule } from '../pages/order-entry/order-entry.module';

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    SharedComponentsModule,
    OrderEntryModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
