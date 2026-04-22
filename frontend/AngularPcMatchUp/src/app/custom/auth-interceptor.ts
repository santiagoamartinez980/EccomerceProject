import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  console.log('Interceptando la solicitud HTTP:', req);
  if(req.url.indexOf("Acceso")>0)return next(req);
  const token = localStorage.getItem('token');
  const clonRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
  return next(clonRequest);
};
