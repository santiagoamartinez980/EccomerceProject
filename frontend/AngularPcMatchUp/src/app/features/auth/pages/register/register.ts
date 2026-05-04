import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthApiService } from '../../services/auth-api.service';
import { Router } from '@angular/router';

import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import {MatCardModule} from '@angular/material/card';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-register',
  imports: [MatCardModule, MatFormFieldModule, MatInputModule,MatButtonModule, 
    MatIconModule, MatSnackBarModule,ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private snack = inject(MatSnackBar);
  public hidePassword = true;
  private authApiService=inject(AuthApiService);
  private router=inject(Router);
  public formBuil =inject(FormBuilder);

  public formRegistro :FormGroup=this.formBuil.group({ 
    nombre:['',Validators.required],
    apellidos:['',Validators.required],
    correo:['',Validators.required],
    clave:['',Validators.required]
  });
  registrarse(){
    if(this.formRegistro.invalid)return;
    const objeto = {
      nombre: this.formRegistro.value.nombre,
      apellidos: this.formRegistro.value.apellidos,
      correo: this.formRegistro.value.correo,
      clave: this.formRegistro.value.clave
    };
    this.authApiService.register(objeto).subscribe({
      next:(data)=>{
          this.snack.open('¡Registro exitoso! Bienvenido 🎉', 'OK', {
            duration: 3000,
            panelClass: 'snack-success'
          });
        setTimeout(() => this.router.navigate(['']), 1500);

      },
      error:(err)=>{
        const mensaje = err?.error?.message ?? 'Error al registrarse, intenta de nuevo';
        this.snack.open(mensaje, 'Cerrar', {
        duration: 4000,
        panelClass: 'snack-error'
        });
      }
    });
  }
  volver(){
    this.router.navigate(['']);
  }

}
