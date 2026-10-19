# Ssal.Application

Acá viven los casos de uso (una clase por acción de negocio) y las interfaces que
`Ssal.Infrastructure` implementa (`IAnalisisRepository`, etc.).

**Análisis** es el primer módulo migrado (piloto de la Fase 2): ver carpeta `Analisis/`.
El resto de los módulos todavía acceden a los datos directo desde `Ssal.Api` con
`SsalDbContext` — se van a ir migrando de a uno, siguiendo el mismo patrón.
