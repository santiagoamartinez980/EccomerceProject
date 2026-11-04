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
import { I18nService } from '../../../../core/i18n/i18n.service';
import { TranslatePipe } from '../../../../core/i18n/translate.pipe';
@Component({
  selector: 'app-register',
  imports: [MatCardModule, MatFormFieldModule, MatInputModule,MatButtonModule, 
    MatIconModule, MatSnackBarModule,ReactiveFormsModule, TranslatePipe],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private snack = inject(MatSnackBar);
  private i18n = inject(I18nService);
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
          this.snack.open(this.i18n.t('register.success'), this.i18n.t('common.ok'), {
            duration: 3000,
            panelClass: 'snack-success'
          });
        setTimeout(() => this.router.navigate(['']), 1500);

      },
      error:(err)=>{
        const mensaje = err?.error?.message ?? this.i18n.t('register.error');
        this.snack.open(mensaje, this.i18n.t('common.close'), {
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
