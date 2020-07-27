import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { OrderEntryComponent } from '../pages/order-entry/order-entry.component';
import { ComposerMedComponent } from '../pages/composer-med/composer-med.component';
import { ComposerMedSelfComponent } from '../pages/composer-med-self/composer-med-self.component';
import { PatientsDashboardComponent} from '../pages/patients-dashboard/patients-dashboard.component';
import { ErrorPageComponent } from '../shared/component/error-page/error-page.component';

const routes: Routes = [
    { path: '', redirectTo: 'patients/1/orders', pathMatch: 'full' },
    { path: 'patients', component: PatientsDashboardComponent, data: {title: 'Department'}},
    { path: 'patients/:id/orders/:idOrder', component: ComposerMedComponent, data: {title: 'Composer'} },
    { path: 'patients/:id/orders', component: OrderEntryComponent, data: {title: 'Medication Services'} },
    { path: 'patients/:id/new-order/:idMed', component: ComposerMedSelfComponent, data: {title: 'Composer'} },
    { path: 'not-found', component: ErrorPageComponent, data: {message: 'Page not found!'} },
    { path: '**', redirectTo: '/not-found'}
];
  
@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule]
})
export class AppRoutingModule {

}
