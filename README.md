# API DGII SaaS - Facturación Electrónica (Dominicana)

Plataforma SaaS multi-tenant diseñada para la emisión, firma digital y recepción de comprobantes fiscales electrónicos (e-CF) en cumplimiento con las normativas de la DGII (República Dominicana).

## 🚀 Características Principales

- **Arquitectura Limpia (Clean Architecture)**: Separación estricta de responsabilidades (Domain, Application, Infrastructure, Api).
- **Multi-Tenancy**: Aislamiento de datos por Tenant mediante filtros globales de EF Core y resolución por cabecera `X-Tenant-Id`.
- **Firma Digital (XAdES)**: Implementación de firmado de XML utilizando certificados `.pfx` / `.p12`.
- **Procesamiento Asíncrono**: Integración con **Hangfire** para el envío de documentos y reintentos automáticos.
- **Documentación Premium**: Interfaz de API interactiva mediante **Scalar** (sucesor moderno de Swagger).
- **Observabilidad**: Logs estructurados con **Serilog**.

## 🛠️ Stack Tecnológico

- **Framework**: .NET 10 (C#)
- **Persistencia**: Entity Framework Core + SQL Server
- **Messaging**: MediatR (Patrón Command/Query)
- **Validación**: FluentValidation
- **Background Jobs**: Hangfire
- **Caching/Locks**: Abstracción para Hazelcast (incluye implementaciones dummy para desarrollo local).

## 📂 Estructura del Proyecto

```text
src/
├── DgiiSaas.Api             # Punto de entrada, Controladores y Middleware
├── DgiiSaas.Application     # Lógica de negocio, Interfaces y Comandos (MediatR)
├── DgiiSaas.Domain          # Entidades, Enums y Excepciones de Dominio
├── DgiiSaas.Infrastructure  # Implementación de persistencia, firma y servicios externos
├── DgiiSaas.Shared          # DTOs y utilidades compartidas
└── DgiiSaas.Workers         # Procesos de fondo (Background Workers)
```

## 🏁 Inicio Rápido

### Requisitos Previos
- .NET 10 SDK
- SQL Server (LocalDB o Express)

### Configuración de Base de Datos
1. Actualiza la cadena de conexión en `src/DgiiSaas.Api/appsettings.json`.
2. Aplica las migraciones:
   ```powershell
   dotnet ef database update --project src/DgiiSaas.Infrastructure --startup-project src/DgiiSaas.Api
   ```

### Ejecución
```powershell
cd src/DgiiSaas.Api
dotnet run --urls http://localhost:5005
```

Accede a la documentación en: `http://localhost:5005/scalar/v1`

## 🧪 Pruebas y Semilla (Seed)

El proyecto incluye un `TestController` para facilitar la configuración inicial:
1. **POST `/api/v1/Test/seed`**: Crea un tenant de prueba (`RNC 130000000`) y carga un certificado dummy.
2. **POST `/api/v1/Test/test-signature`**: Permite verificar el motor de firma digital enviando un XML simple.

## ⚖️ Licencia

Este proyecto es un prototipo funcional para propósitos de desarrollo y arquitectura.

---
*Desarrollado con ❤️ para el ecosistema fiscal de la República Dominicana.*
