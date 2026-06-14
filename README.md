# BakeryPay

Sistema de gestion de pagos a proveedores para una empresa panificadora, con backend ASP.NET Core y aplicacion movil .NET MAUI bajo arquitectura MVVM.

## Arquitectura

- `src/BakeryPay.Domain`: entidades, enums y reglas base del dominio.
- `src/BakeryPay.Application`: DTOs, contratos, servicios y logica de negocio.
- `src/BakeryPay.Infrastructure`: persistencia con EF Core para SQL Server, repositorios, JWT y hashing.
- `src/BakeryPay.Api`: REST API, controllers, configuracion HTTP, autenticacion y autorizacion.
- `src/BakeryPay.Mobile`: app movil MAUI con MVVM, Shell navigation, consumo de API y biometria nativa.
- `sql`: scripts de creacion y datos semilla.

## Modulos implementados

- Autenticacion con login tradicional y login biometrico.
- Gestion de usuarios y roles.
- Gestion de proveedores.
- Gestion de pagos.
- Gestion de comprobantes.
- Notificaciones.
- Dashboard y consultas.

## Roles

- `Administrator`
- `Cashier`
- `Provider`

## Scripts SQL

Ejecutar los scripts en este orden:

1. `sql/01_CreateDatabase.sql`
2. `sql/02_SeedData.sql`

Usuarios de demostracion:

- `admin@bakerypay.com`
- `cajera@bakerypay.com`
- `proveedor@bakerypay.com`

Contrasena comun:

- `12345678`

## Backend

- Proyecto principal: `src/BakeryPay.Api/BakeryPay.Api.csproj`
- Ajustar `ConnectionStrings:DefaultConnection`, `Jwt` y `Smtp` en `src/BakeryPay.Api/appsettings.json`
- Proveedor EF Core configurado para SQL Server con `Microsoft.EntityFrameworkCore.SqlServer`

Configuracion SMTP:

- `Smtp:Enabled`: activa o desactiva el envio real de correos
- `Smtp:Host` y `Smtp:Port`: servidor SMTP
- `Smtp:UseSsl`: habilita TLS/SSL
- `Smtp:UseDefaultCredentials`: usar credenciales del sistema si aplica
- `Smtp:Username` y `Smtp:Password`: credenciales SMTP
- `Smtp:FromEmail` y `Smtp:FromName`: remitente visible

Cuando se crea un proveedor desde la API, BakeryPay crea tambien su usuario asociado, genera una clave temporal y trata de enviarle el correo con esas credenciales.

## App movil

- Proyecto: `src/BakeryPay.Mobile/BakeryPay.Mobile.csproj`
- Requiere workloads MAUI instalados, al menos `maui-android`
- Incluye paginas XAML, viewmodels MVVM, servicios HTTP, almacenamiento seguro de sesion y biometria nativa

## Nota

Este repositorio se entrega sin archivos temporales de compilacion ni logs del entorno local.
