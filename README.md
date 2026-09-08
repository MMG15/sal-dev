# SSAL — Sistema de Seguimiento de Análisis de Laboratorio

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
- JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) + BCrypt — autenticación y hash de contraseñas
- [QuestPDF](https://www.questpdf.com/) — generación de los informes de resultados en PDF

**Frontend**
- [React 19](https://react.dev/) + [TypeScript](https://www.typescriptlang.org/)
- [Vite](https://vitejs.dev/) — bundler y servidor de desarrollo
- React Router — ruteo de la SPA
- CSS Modules — estilos por componente (sin librería de UI externa)

**Base de datos**
- PostgreSQL 17

**Otros**
- EF Core Migrations (`dotnet ef`) para versionar el esquema de la base
- Documento de requisitos del proyecto en [`documentos/ssal.md`](documentos/ssal.md) (RF, casos de uso, roles, diagramas)

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

### Módulos funcionales
- **Autenticación** — login con JWT.
- **Clientes y Empresas** — alta/edición, datos fiscales (CUIT/DNI, condición frente al IVA) y detección de duplicados.
- **Consultas** — registro por canal (mail, WhatsApp, teléfono, presencial), selección de cliente/empresa responsable, estados (recibida, respondida, aceptada, tercerizada, eliminada) y aviso de consultas pendientes de respuesta.
- **Presupuestos** — armado a partir del catálogo de análisis, cargo adicional de asesoramiento (con tipo: consulta / terreno / proceso completo), archivos adjuntos, envío por email.
- **Análisis** — catálogo por grupos y área (MIC/FQ), con unidad de medida y rango o valor de referencia para detectar resultados fuera de rango.
- **SSE / Rótulos** — alta de la solicitud de servicio, asignación de rótulo interno con detección de duplicados, registro de muestra (incluyendo rechazo de muestra si no llega en condiciones), corrección de rótulo con confirmación obligatoria y auditoría completa de cambios.
- **Resultados** — carga de resultados por análisis, alerta automática de valores fuera de rango, validación o rechazo por el responsable de área, y generación de un informe en PDF una vez validados todos los resultados de una muestra.

### Rediseño visual
Identidad visual propia del laboratorio (paleta institucional, marca en el sidebar, dashboard
de inicio por módulo) y ajustes generales de legibilidad.

### Pendiente
- Facturación
- Firma digital (doble validación)
- Gestión de usuarios desde la interfaz (hoy solo existe el seed inicial)
- Portal del cliente externo
- Autorización por rol a nivel de backend (hoy el control de acceso por rol es solo del lado del frontend)
