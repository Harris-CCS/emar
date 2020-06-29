import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { OrderEntryComponent } from '../pages/order-entry/order-entry.component';
import { MedComposerComponent } from '../pages/med-composer/med-composer.component';
import { PatientsDashboardComponent} from '../pages/patients-dashboard/patients-dashboard.component';
import { ErrorPageComponent } from '../shared/component/error-page/error-page.component';

const routes: Routes = [
    { path: '', redirectTo: './patients', pathMatch: 'full' },
    { path: 'patients', component: PatientsDashboardComponent},
    { path: 'patients/:id/orders/:idOrder', component: MedComposerComponent },
    { path: 'patients/:id/orders', component: OrderEntryComponent },
    { path: 'patients/:id/new-order', component: MedComposerComponent },
    { path: 'not-found', component: ErrorPageComponent, data: {message: 'Page not found!'} },
    { path: '**', redirectTo: '/not-found'}
];
  
@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {

}
