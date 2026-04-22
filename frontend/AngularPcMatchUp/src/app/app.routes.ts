import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login-component/login-component';
import { Registro } from './pages/registro/registro';
import { Iniciocomponent } from './pages/inicio/inicioComponent';
import { authGuard } from './custom/auth-guard';

export const routes: Routes = [
    {path:"",component:LoginComponent},
    {path:"Registro",component:Registro},
    {path: "inicio", component: Iniciocomponent, canActivate: [authGuard]}
];
