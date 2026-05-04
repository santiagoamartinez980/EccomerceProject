import { HttpInterceptorFn } from '@angular/common/http';
//20-04-2026 se agrega el interceptor para agregar el token en las solicitudes HTTP, excepto en la solicitud de login
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
