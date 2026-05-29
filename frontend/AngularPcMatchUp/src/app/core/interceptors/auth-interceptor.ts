import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  console.log('Interceptando la solicitud HTTP:', req.url);
  
  // No agregar token a endpoints de autenticación
  if (req.url.includes('Access') || req.url.includes('login') || req.url.includes('register')) {
    return next(req);
  }

  const token = localStorage.getItem('token');
  
  // Solo clonar si hay token
  if (!token) {
    console.warn('⚠️ No hay token en localStorage');
    return next(req);
  }

  console.log('✓ Agregando token a la solicitud');
  const clonedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
  
  return next(clonedRequest);
};
