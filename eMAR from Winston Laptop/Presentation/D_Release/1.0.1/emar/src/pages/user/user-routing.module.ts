import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { UserComponent } from './user.component';
import { PatientsDashboardComponent } from '../patients-dashboard/patients-dashboard.component';
import { DepartmentDashboardComponent } from '../department-dashboard/department-dashboard.component';

const routes: Routes = [
    {
        path: '',
        component: UserComponent,
        children: [

            // { path: 'patients', component: PatientsDashboardComponent, data: {title: 'Department'} },
            { path: 'patients', component: DepartmentDashboardComponent, data: { title: 'MAR - Department' } },
            { path: 'patients/:patientId', loadChildren: () => import(`../patient/patient.module`).then(m => m.PatientModule) }
        ]
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class UserRoutingModule {

}
