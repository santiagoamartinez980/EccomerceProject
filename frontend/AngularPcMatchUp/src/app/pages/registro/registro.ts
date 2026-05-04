/*
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Acceso } from '../../features/auth/services/';
import { Router } from '@angular/router';

import {MatCardModule} from '@angular/material/card';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatInputModule} from '@angular/material/input';
import {MatButtonModule} from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';



@Component({
  selector: 'app-registro',
  imports: [MatCardModule, MatFormFieldModule, MatInputModule,MatButtonModule, 
    MatIconModule, ReactiveFormsModule],
  templateUrl: './registro.html',
  styleUrl: './registro.css',
})
export class Registro {
  public hidePassword = true;
  private accesoService=inject(Acceso);
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
    this.accesoService.registrarse(objeto).subscribe({
      next:(data)=>{
          this.router.navigate(['']);
      },
      error:(err)=>{
        console.error(err);
        alert("Error al registrarseee");
      }
    });
  }
  volver(){
    this.router.navigate(['']);
  }

}
*/