# Café UNA — Backend

Esta es la parte que no se ve, la cual está detrás de la página web. Recibe las peticiones del frontend, guarda la información en la base de datos y devuelve lo que hace falta para que el sitio funcione.

## ¿Qué hace?

Se encarga de la lógica y los datos del proyecto Café UNA:

### Usuarios y acceso

- **Registro e inicio de sesión** con correo y contraseña.
- **Códigos por correo** para confirmar el registro o recuperar la contraseña.
- **Tokens de sesión** para saber quién está logueado y si es administrador o usuario normal.
- **Renovación de sesión** mientras la persona sigue usando la app.

### Contenido del sitio

- Información de la **página principal** y de **sobre nosotros**.
- **Productos:** nombre, precio, stock, imágenes, etc.
- **Perfil** de cada usuario (nombre, correo, fotos y rol).

### Otras funciones

- **Voluntariado:** Enviar solicitudes.
- **Consulta de cédula** (cuando el checkout lo necesita).
- **Gestión administrativa** para los administradores-->En esta podemos editar y visualizar cosas que un usuario no puede.

Todo esto queda guardado en una **base de datos** para que no se pierda al cerrar el navegador.

## Equipo
Samir Campos Diaz
Fátima Carrillo García
María del Mar Díaz Ruiz
Yeisson Alberto Villalobos Toruño. 
