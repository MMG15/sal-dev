# Matriz de permisos por rol — documento temporal

> ⚠️ **Temporal.** Refleja lo implementado hasta el momento para poder probar el
> sistema con distintos usuarios. No es la matriz de permisos definitiva del
> proyecto — se va a ir ajustando a medida que se terminen los módulos que
> todavía son placeholder.

## Usuarios de prueba sembrados

Los 9 usuarios están sembrados automáticamente al levantar el backend
(`backend/Program.cs`). Contraseña `Admin1234!` para el admin, `Ssal1234!` para
el resto.

| Usuario | Rol |
|---|---|
| `admin@ssal.uccuyo.edu.ar` | ROL-01 — Admin general / Director Técnico |
| `administrativo@ssal.uccuyo.edu.ar` | ROL-02 — Administrativo |
| `presupuestos@ssal.uccuyo.edu.ar` | ROL-03 — Presupuestos |
| `cobranzas@ssal.uccuyo.edu.ar` | ROL-04 — Cobranzas |
| `cliente@ssal.uccuyo.edu.ar` | ROL-05 — Cliente externo |
| `sysadmin@ssal.uccuyo.edu.ar` | ROL-06 — Admin de sistema |
| `analista@ssal.uccuyo.edu.ar` | ROL-07 — Analista |
| `resp.mic@ssal.uccuyo.edu.ar` | ROL-08 — Responsable de área MIC |
| `resp.fq@ssal.uccuyo.edu.ar` | ROL-09 — Responsable de área FQ |

## Matriz de acceso por módulo

Todos los roles ven además **Inicio** (panel general), omitido de la tabla por
ser común a todos. Los módulos marcados 🔒 todavía son placeholder — el rol ya
está habilitado para el día que se construya la pantalla real, pero hoy no hay
nada que ver ahí.

| Rol | Clientes | Consultas | Presupuestos | SSE/Rótulos | Análisis | Resultados | Facturación 🔒 | Firma digital 🔒 | Usuarios 🔒 | Portal 🔒 |
|---|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|:-:|
| ROL-01 Admin general | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| ROL-02 Administrativo | ✅ | ✅ | 👁️¹ | ✅ | ✅ | — | — | — | — | — |
| ROL-03 Presupuestos | — | ✅ | ✅ | — | ✅ | — | — | — | — | — |
| ROL-04 Cobranzas | — | — | — | — | — | — | ✅ | — | — | — |
| ROL-05 Cliente externo | — | — | — | — | — | — | — | — | — | ✅ |
| ROL-06 Admin de sistema | — | — | — | — | — | — | — | — | ✅ | — |
| ROL-07 Analista | ✅ | ✅ | — | ✅ | ✅ | ✅ | — | — | — | — |
| ROL-08 Resp. área MIC | — | — | — | — | ✅ | ✅ | — | ✅ | — | — |
| ROL-09 Resp. área FQ | — | — | — | — | ✅ | ✅ | — | ✅ | — | — |

¹ Administrativo puede **ver** presupuestos aceptados (para vincularlos a una
SSE) pero no puede crearlos ni modificarlos — queda exclusivo de ROL-01/03.

## Dónde está implementado

- **Frontend** (qué pantallas se ven): `frontend/src/config/modulos.ts`
- **Backend** (qué endpoints aceptan cada rol, exigido de verdad vía JWT):
  `[Authorize(Roles = "...")]` en cada controller de `backend/Controllers/`.
  `PresupuestosController` es el único con permisos separados por acción
  (lectura vs. gestión), el resto restringe el controller completo.
