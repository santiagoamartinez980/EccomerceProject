import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthApiService } from '../../services/auth-api.service';

import { Router } from '@angular/router';
import { LoginRequest  } from '../../interfaces/login-request.interface';
import { TokenService } from '../../../../core/services/token.service';


import {MatCardModule} from '@angular/material/card';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { jwtDecode } from 'jwt-decode';

interface JwtPayload {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  role?: string;
}

@Component({
  selector: 'app-login',
  imports: [MatCardModule, MatFormFieldModule, MatInputModule,MatButtonModule, 
    MatIconModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  public hidePassword = true;
  private authApiService=inject(AuthApiService);
  private router=inject(Router);
  public formBuil =inject(FormBuilder);
  private tokenService=inject(TokenService);

  public formLogin :FormGroup=this.formBuil.group({   
    correo:['',Validators.required],
    clave:['',Validators.required]
  });
  
  inisiarSesion(){
    if(this.formLogin.invalid)return;
    const object: LoginRequest = {
      correo: this.formLogin.value.correo,
      clave: this.formLogin.value.clave
    };
    this.authApiService.login(object).subscribe({
      next:(data)=>{
          this.tokenService.setToken(data.value.token);
          //console.log('ROL:', this.tokenService.getRole());
          //console.log('isAdmin:', this.tokenService.isAdmin());

          if (this.tokenService.isAdmin()) {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/productos']);
          }


      },
      error:(err)=>{
        console.error(err);
        alert("Error al iniciar sesión");
        console.error(err);
      }
    });   
  }
  
  registrarse(){
    this.router.navigate(['/auth/registro']);
  }
}
