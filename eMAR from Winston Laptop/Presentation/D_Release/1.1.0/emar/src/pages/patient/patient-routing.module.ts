import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { PatientComponent } from './patient.component';
import { OrderEntryComponent } from '../order-entry/order-entry.component'
import { PatientDashboardComponent } from '../patient-dashboard/patient-dashboard.component';
import { ComposerMedComponent } from '../composer-med/composer-med.component';
import { ComposerMedSelfComponent } from '../composer-med-self/composer-med-self.component';
import { ComposerMainComponent } from '../composer-main/composer-main.component'

const routes: Routes = [
    {
        path: '',
        component: PatientComponent,
        children: [
            { path: '', component: PatientDashboardComponent, data: { title: 'MAR - Patient' } },
            { path: ':dest', component: OrderEntryComponent, data: { title: 'Medication Services' } },

            { path: 'orders/:idOrder', component: ComposerMedComponent, data: { title: 'Composer' } }, //do we need this?
            { path: 'new-order/:idMed', component: ComposerMedSelfComponent, data: { title: 'Composer' } }, //do we need this?

            { path: ':dest/new-order/:medId', component: ComposerMainComponent, data: { title: 'New Orders' } },
            { path: ':dest/update-order/:medId', component: ComposerMainComponent, data: { title: 'Update Order' } },
        ]
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class PatientRoutingModule {

}
