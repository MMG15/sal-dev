# SAL — Sistema de Seguimiento de Análisis de Laboratorio

> ⚠️ **Documento temporal de trabajo.** Resume el estado del proyecto a la fecha para
> facilitar el seguimiento y la puesta en marcha local. No reemplaza una documentación
> final del sistema.

Sistema de gestión para el **Laboratorio Control de Calidad "Dr. Alberto Graffigna" — UCCuyo**,
que reemplaza al sistema anterior (BQ3). Cubre el circuito completo desde que un cliente
consulta hasta la entrega del informe de resultados.

---

## Herramientas y stack utilizado

**Backend**
- [ASP.NET Core .NET 10](https://dotnet.microsoft.com/) — API REST
- [Entity Framework Core](https://learn.microsoft.com/ef/core/) + [Npgsql](https://www.npgsql.org/) — acceso a datos sobre PostgreSQL
- JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + BCrypt — autenticación, hash de contraseñas y autorización real por rol en cada acción del backend (no solo en el frontend)
- [QuestPDF](https://www.questpdf.com/) — generación de Presupuestos, SSE e Informes en PDF, con un estilo visual compartido y firmas digitales reales
- [MailKit](https://github.com/jstedfan/MailKit) — importación de consultas por IMAP y envío de presupuestos/informes por SMTP

**Frontend**
- [React 19](https://react.dev/) + [TypeScript](https://www.typescriptlang.org/)
- [Vite](https://vitejs.dev/) — bundler y servidor de desarrollo
- React Router — ruteo de la SPA
- [Mantine](https://mantine.dev/) — librería de componentes para los módulos rediseñados (Consultas, Presupuestos, SSE, Resultados, Firma); los módulos más antiguos todavía usan CSS Modules propios

**Base de datos**
- PostgreSQL 17

**Arquitectura del backend**
4 proyectos .NET con Clean Architecture (regla de dependencias forzada por el compilador):
`Ssal.Domain` (entidades y lógica pura) → `Ssal.Application` (casos de uso + interfaces de
repositorio) → `Ssal.Infrastructure` (EF Core, MailKit, QuestPDF — implementaciones concretas) →
`Ssal.Api` (controllers delgados). Solución en `backend/Ssal.slnx`.

**Otros**
- EF Core Migrations (`dotnet ef`) para versionar el esquema de la base
- Documento de requisitos del proyecto en [`documentos/ssal.md`](documentos/ssal.md) (RF, casos de uso, roles, diagramas)
- Matriz de permisos por rol y usuarios de prueba en [`documentos/PERMISOS_TEMPORAL.md`](documentos/PERMISOS_TEMPORAL.md)

---

## Cómo levantar el proyecto en local

### Requisitos previos
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (18+)
- PostgreSQL 17 corriendo en `localhost:5432`, con una base `ssal_db` creada

### 1. Backend

```bash
cd backend

# Configurar la contraseña real de Postgres (no se versiona en appsettings.json)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=ssal_db;Username=postgres;Password=TU_PASSWORD"

dotnet run
```

El backend queda escuchando en `http://localhost:5044`. Al iniciar aplica automáticamente
las migraciones pendientes y siembra datos base (roles, usuarios de prueba, catálogo de análisis).

### 2. Frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend queda en `http://localhost:5173` (proxy configurado hacia el backend).

### 3. Ingresar al sistema

Usuario administrador de prueba (sembrado automáticamente):

- **Email:** `admin@ssal.uccuyo.edu.ar`
- **Contraseña:** `Admin1234!`

> Hay un usuario de prueba sembrado por cada rol del sistema (Administrativo, Presupuestos,
> Cobranzas, Analista, Responsables de área MIC/FQ, etc.) — ver `Program.cs` para la lista completa.
> Estas credenciales son solo para desarrollo.

---

## Qué está hecho hasta ahora

Los 11 módulos del documento de requisitos están implementados y conectados a backend real;
ningún módulo queda como placeholder.

### Módulos funcionales
- **Autenticación** — login con JWT, autorización real por rol en cada endpoint del backend.
- **Clientes y Empresas** — alta/edición, datos fiscales (CUIT, condición frente al IVA) y detección de duplicados.
- **Correo entrante** — bandeja que importa automáticamente las consultas que llegan por mail (IMAP), con botón de revisión manual, triage antes de convertir a Consulta formal, y detección de respuestas del cliente (aceptación/rechazo de presupuesto) por las cabeceras del mail para vincularlas a la consulta original en vez de crear una nueva.
- **Consultas** — seguimiento por canal de ingreso, cliente/empresa asociada, estado, y aviso de consultas pendientes de respuesta hace más de cierta cantidad de días.
- **Presupuestos** — armado seleccionando análisis sueltos y/o Paquetes del catálogo, selección de Métodos aplicados, cargo adicional de asesoramiento (consulta / terreno / proceso completo), selector de qué moneda mostrar en el PDF (USD, ARS o ambas), archivos adjuntos propios o tomados de la biblioteca de Documentos institucionales, envío por email con el PDF adjunto.
- **Análisis** — catálogo organizado en 4 pestañas: análisis individuales (precio, unidad, rango/valor de referencia, método, si es tercerizado), Paquetes (combos con precio propio bonificado), Categorías (norma, tipo y cantidad de muestra) y Métodos (catálogo reutilizable de técnicas/normas/equipos).
- **SSE / Rótulos** — alta de la solicitud de servicio, uno o varios rótulos por SSE (para pedidos con muestras repetidas, con sufijo de letra), numeración de rótulo por día juliano (`AA-DDD-NNN`), registro de muestra (incluyendo rechazo si no llega en condiciones), corrección de rótulo con motivo obligatorio y auditoría completa de cambios.
- **Resultados** — carga de resultados por análisis (incluyendo los que llegan dentro de un paquete), alerta automática de valores fuera de rango, edición de un valor ya cargado antes de validar, validación o rechazo por el responsable de área, y generación del informe en PDF una vez validados todos los resultados de una muestra.
- **Facturación** — registro de la factura emitida en TANGO (número y fecha) vinculada a la SSE; listado de facturas adeudadas agrupadas por cliente y marca de cobro. La anulación con motivo queda pendiente.
- **Firma digital** — doble validación sobre informes y presupuestos: firma del responsable de cada área (con contraseña) y aprobación final del administrador. Rechazo con motivo y reenvío. Los PDF muestran solo las firmas ya concretadas, con la fecha de emisión del documento; sin aprobación completa salen marcados como borrador.
- **Usuarios** — alta, modificación, activación/desactivación de cuentas con contraseña temporal, gestión de las cuentas de correo desde las que se importan consultas, biblioteca de Documentos institucionales (certificaciones, procedimientos) reutilizable desde cualquier presupuesto, y registro de auditoría de accesos y acciones críticas.
- **Portal del cliente** — acceso separado del login interno; el cliente ve el estado de sus muestras y descarga solo los informes ya aprobados.

### Rediseño visual
Identidad visual propia del laboratorio. Los módulos más grandes (Consultas, Presupuestos, SSE,
Resultados, Firma) ya migraron a Mantine, con sidebar agrupado por área, breadcrumbs y búsqueda
global (Ctrl+K); Clientes, Análisis, Facturación y Usuarios quedan con CSS Modules clásico.

### Pendiente
- Anulación de facturas con motivo (RF-25)
- Facturación centralizada de varias SSE (RF-28)
- Ingreso de consultas por WhatsApp (camino de implementación ya investigado, bloqueado hasta contar con el número de WhatsApp Business del laboratorio)
- Validar el modelo de Análisis contra PDF reales de alimentos (hoy solo validado con pedidos de agua)
- Despliegue a producción en el servidor del laboratorio (plan de 6 etapas ya definido, pospuesto a propósito hasta terminar de ajustar el producto con el personal real del laboratorio)
