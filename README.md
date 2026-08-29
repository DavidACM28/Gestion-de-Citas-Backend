# Sistema de Gestión de Citas Médicas

API REST desarrollada con **ASP.NET Core Minimal APIs y .NET 10** para la gestión de citas de una clínica.

El sistema permite administrar usuarios, pacientes, médicos, especialidades, horarios de atención, citas y bloqueos de agenda. Incluye autenticación mediante JWT, autorización por roles, persistencia con Entity Framework Core, operaciones transaccionales y un proceso maestro-detalle para evitar conflictos de horarios.

---

## Descripción del caso

Una clínica necesita centralizar la gestión de sus citas médicas y evitar problemas comunes como:

- Reservar dos citas para un mismo médico en el mismo horario.
- Registrar citas fuera del horario de atención del médico.
- Crear citas durante períodos bloqueados.
- Permitir que pacientes modifiquen citas que no les pertenecen.
- Gestionar incorrectamente los estados de una cita.
- Crear bloqueos de agenda que afecten citas existentes.
- Perder consistencia de datos al realizar operaciones que afectan varias tablas.

La aplicación implementa reglas de negocio para resolver estos problemas mediante horarios de atención, bloqueos de agenda y un sistema de **slots de 15 minutos**.

---

# Tecnologías utilizadas

## Backend

- .NET 10
- C#
- ASP.NET Core
- Minimal APIs
- Entity Framework Core
- LINQ
- JWT Bearer Authentication
- BCrypt
- Mapster
- Swagger / OpenAPI
- Dependency Injection

## Base de datos

- SQL Server
- Entity Framework Core Migrations
- Azure SQL Database para producción

## Despliegue

- Docker
- Render para la API
- Azure SQL Database para la base de datos

---

# Arquitectura de la solución

La solución se encuentra dividida en diferentes proyectos con responsabilidades independientes:

```text
Gestion-de-Citas-Backend/

├── Gestion.Citas.API/
│   ├── Endpoints/
│   ├── Middleware/
│   ├── Program.cs
│   └── appsettings.json
│
├── Gestion.Citas.Business/
│   ├── DTO/
│   ├── Interfaces/
│   ├── Implementations/
│   └── Constants/
│
├── Gestion.Citas.Repositories/
│   ├── Interfaces/
│   └── Implementations/
│
├── Gestion.Citas.DataAccess/
│   ├── Entities/
│   ├── Configurations/
│   ├── Migrations/
│   └── AppointmentsDbContext.cs
│
├── Gestion.Citas.Common/
│
├── Dockerfile
├── docker-compose.yml
└── Gestion.Citas.slnx
```

## Responsabilidades

### Gestion.Citas.API

Contiene la capa de presentación de la aplicación.

Responsabilidades principales:

- Definición de Minimal APIs.
- Rutas y verbos HTTP.
- Autenticación y autorización.
- Configuración de Swagger.
- Middleware para manejo global de excepciones.
- Obtención de Claims del usuario autenticado.

### Gestion.Citas.Business

Contiene la lógica de negocio.

Aquí se realizan validaciones como:

- Existencia de médicos y pacientes.
- Validación de horarios.
- Reglas para crear citas.
- Validación de bloqueos.
- Transiciones de estado.
- Permisos según usuario.
- Generación de slots.
- Políticas de cancelación.

También contiene los DTO utilizados en requests y responses.

### Gestion.Citas.Repositories

Contiene el acceso a datos.

Responsabilidades:

- Consultas mediante Entity Framework Core.
- Operaciones CRUD.
- Consultas mediante LINQ.
- Operaciones transaccionales.
- Persistencia de citas y slots.
- Consultas filtradas.

### Gestion.Citas.DataAccess

Contiene:

- Entidades de base de datos.
- `DbContext`.
- Configuraciones de Entity Framework Core.
- Relaciones.
- Índices.
- Restricciones.
- Migraciones.

### Gestion.Citas.Common

Contiene componentes reutilizables por los diferentes proyectos de la solución.

---

# Flujo general de la aplicación

La arquitectura sigue aproximadamente el siguiente flujo:

```text
HTTP Request
     ↓
Minimal API Endpoint
     ↓
Business Service
     ↓
Repository
     ↓
Entity Framework Core
     ↓
SQL Server
```

Esto permite separar la lógica HTTP, las reglas de negocio y el acceso a datos.

---

# Entidades principales

El sistema utiliza las siguientes entidades:

| Entidad | Descripción |
|---|---|
| `User` | Usuario que puede autenticarse en el sistema |
| `Patient` | Paciente de la clínica |
| `Doctor` | Médico registrado |
| `Specialty` | Especialidad médica |
| `BusinessHours` | Horario semanal de atención de un médico |
| `Appointment` | Cita médica |
| `AppointmentSlot` | Fragmento de 15 minutos ocupado por una cita |
| `AppointmentBlock` | Bloqueo temporal de la agenda de un médico |

---

# Relaciones principales

```text
User
 ├── Patient
 └── Doctor

Specialty
    │
    └── Doctor
          │
          ├── BusinessHours
          ├── Appointment
          └── AppointmentBlock

Patient
    │
    └── Appointment
              │
              └── AppointmentSlot
```

---

# Proceso maestro-detalle

Uno de los principales procesos de negocio de la aplicación es la creación de citas utilizando una operación **maestro-detalle**.

## Maestro

La entidad:

```text
Appointment
```

representa la información principal de la cita:

- Médico.
- Paciente.
- Fecha.
- Hora de inicio.
- Duración.
- Estado.
- Motivo.
- Nota.

## Detalle

La entidad:

```text
AppointmentSlot
```

representa cada intervalo de 15 minutos ocupado por la cita.

La relación es:

```text
Appointment 1 ───────── N AppointmentSlot
```

## Ejemplo

Una cita de 45 minutos:

```text
Fecha: 2026-09-01
Hora: 09:00
Duración: 45 minutos
```

genera:

```text
Appointment
    │
    ├── AppointmentSlot 09:00
    ├── AppointmentSlot 09:15
    └── AppointmentSlot 09:30
```

La cita y sus respectivos slots se crean dentro de una misma transacción.

```text
BEGIN TRANSACTION

Crear Appointment

Crear AppointmentSlot 09:00
Crear AppointmentSlot 09:15
Crear AppointmentSlot 09:30

COMMIT
```

Si alguno de los registros no puede ser creado:

```text
ROLLBACK
```

Esto garantiza la consistencia de los datos.

---

# Prevención de doble reserva

La tabla de slots posee una restricción única compuesta por:

```text
DoctorId
+
Date
+
Time
```

Conceptualmente:

```text
UNIQUE (DoctorId, Date, Time)
```

De esta manera, dos citas no pueden reservar el mismo bloque de tiempo para un mismo médico.

Por ejemplo:

```text
Cita existente:

09:00
09:15
09:30
```

Una nueva cita que necesite:

```text
09:15
09:30
```

no podrá ser registrada debido a que esos slots ya se encuentran ocupados.

Esta restricción funciona además como protección ante solicitudes concurrentes.

---

# Reglas de negocio de citas

Antes de registrar una cita el sistema valida:

- Existencia del médico.
- Existencia del paciente.
- Permisos del usuario.
- Que la fecha y hora no pertenezcan al pasado.
- Que el médico tenga un horario configurado para ese día.
- Que la cita se encuentre completamente dentro del horario del médico.
- Que no exista un bloqueo de agenda que afecte el rango solicitado.
- Que los slots necesarios se encuentren disponibles.

La duración de la cita se obtiene de la configuración del horario de atención del médico.

---

# Estados de una cita

Una cita puede encontrarse en los siguientes estados:

```text
REQUESTED
CONFIRMED
BEING_ATTENDED
FINISHED
CANCELED
DID_NOT_ATTEND
```

Flujo principal:

```text
REQUESTED
    ↓
CONFIRMED
    ↓
BEING_ATTENDED
    ↓
FINISHED
```

También puede ocurrir:

```text
REQUESTED
    ↓
CANCELED
```

o:

```text
CONFIRMED
    ↓
CANCELED
```

o:

```text
CONFIRMED
    ↓
DID_NOT_ATTEND
```

Las transiciones inválidas son rechazadas por la lógica de negocio.

---

# Cancelación de citas

Las citas únicamente pueden ser canceladas desde estados permitidos.

Para usuarios con rol `Patient` se aplican reglas adicionales:

- Solo pueden cancelar sus propias citas.
- Deben faltar como mínimo 2 horas para el inicio de la cita.

Los usuarios administrativos autorizados pueden realizar la cancelación de acuerdo con sus permisos.

Cuando una cita se cancela:

```text
BEGIN TRANSACTION

Cambiar estado → CANCELED

Eliminar AppointmentSlots

COMMIT
```

Los slots quedan nuevamente disponibles.

---

# Actualización de citas

La modificación de una cita también actualiza los slots asociados.

Flujo:

```text
Validar cita
      ↓
Validar permisos
      ↓
Validar nuevo horario
      ↓
Validar bloqueos
      ↓
Generar nuevos slots
      ↓
BEGIN TRANSACTION
      ↓
Eliminar slots anteriores
      ↓
Crear nuevos slots
      ↓
Actualizar cita
      ↓
COMMIT
```

---

# Bloqueos de agenda

`AppointmentBlock` permite bloquear un rango horario de un médico.

Ejemplo:

```text
Doctor: 3
Fecha: 2026-09-01
Inicio: 10:00
Fin: 12:00
Motivo: Reunión
```

El sistema evita crear otro bloqueo que colisione con el existente.

La regla utilizada para detectar solapamientos es:

```text
inicioExistente < finNuevo
AND
finExistente > inicioNuevo
```

Por ejemplo:

```text
Bloqueo existente:
10:00 -------- 12:00

Nuevo bloqueo:
       11:00 -------- 13:00
```

Existe conflicto.

En cambio:

```text
Bloqueo existente:
10:00 -------- 11:00

Nuevo bloqueo:
               11:00 -------- 12:00
```

no existe conflicto.

---

# Bloqueo forzado

La creación de bloqueos soporta una propiedad:

```text
Force
```

Cuando el bloqueo afecta citas existentes, una creación normal es rechazada.

Un bloqueo forzado autorizado permite realizar el proceso:

```text
BEGIN TRANSACTION

Crear AppointmentBlock

Buscar citas afectadas

Cambiar citas a CANCELED

Eliminar AppointmentSlots asociados

COMMIT
```

Si ocurre un error, la operación completa debe ser revertida.

Este proceso garantiza que no se cree un bloqueo dejando citas activas dentro del mismo rango horario.

---

# Seguridad

La API utiliza:

```text
JWT Bearer Authentication
```

El token contiene Claims utilizados para identificar al usuario y controlar sus permisos:

- Identificador de usuario.
- Username.
- Rol.

---

# Roles

La aplicación utiliza cuatro roles:

```text
Admin
Receptionist
Doctor
Patient
```

## Admin

Posee acceso a las principales operaciones administrativas, incluyendo gestión de usuarios, médicos, especialidades, horarios, citas y bloqueos.

## Receptionist

Puede gestionar información necesaria para la atención de pacientes y operaciones sobre citas de acuerdo con los endpoints autorizados.

## Doctor

Puede consultar información asociada a su cuenta, consultar sus citas, modificar determinados horarios y gestionar bloqueos de su propia agenda.

## Patient

Puede acceder a su propia información y gestionar sus citas de acuerdo con las reglas de negocio establecidas.

---

# Principales endpoints

La URL base local depende del puerto configurado por ASP.NET Core.

Con Docker se utiliza normalmente:

```text
http://localhost:8080
```

---

## Autenticación

| Método | Endpoint | Descripción | Autorización |
|---|---|---|---|
| POST | `/api/auth/login` | Autenticación y generación de JWT | Público |

---

## Usuarios

| Método | Endpoint | Descripción | Roles |
|---|---|---|---|
| POST | `/api/users` | Crear usuario administrativo | Admin |
| GET | `/api/users` | Listar usuarios | Admin |
| GET | `/api/users/{id}` | Consultar usuario por ID | Admin |
| GET | `/api/users/me` | Consultar usuario autenticado | Admin, Receptionist |

---

## Especialidades

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/specialties` | Crear especialidad |
| GET | `/api/specialties` | Listar especialidades |
| GET | `/api/specialties/{id}` | Consultar especialidad por ID |

---

## Pacientes

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/patients` | Registrar paciente |
| GET | `/api/patients` | Listar pacientes |
| GET | `/api/patients/{id}` | Consultar paciente por ID |
| GET | `/api/patients/me` | Consultar paciente autenticado |

---

## Médicos

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/doctors` | Registrar médico |
| GET | `/api/doctors` | Consultar médicos utilizando filtros |
| GET | `/api/doctors/{id}` | Consultar médico por ID |
| GET | `/api/doctors/me` | Consultar médico autenticado |
| GET | `/api/doctors/me/appointments` | Consultar citas del médico autenticado |
| GET | `/api/doctors/{doctorId}/business-hours` | Consultar horarios de un médico |

Filtros disponibles para consulta de médicos:

```text
specialty
name
pageNumber
pageSize
```

---

## Horarios de atención

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/businessHours` | Crear horario |
| GET | `/api/businessHours/{id}` | Consultar horario |
| PUT | `/api/businessHours/{id}` | Actualizar horario |
| DELETE | `/api/businessHours/{id}` | Desactivar horario |

---

## Citas

| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/appointments` | Consultar citas mediante filtros |
| POST | `/api/appointments` | Crear una cita y sus slots |
| PUT | `/api/appointments/{id}` | Modificar cita y actualizar slots |
| PATCH | `/api/appointments/{id}/confirm` | Confirmar cita |
| PATCH | `/api/appointments/{id}/start` | Iniciar cita |
| PATCH | `/api/appointments/{id}/finish` | Finalizar cita |
| PATCH | `/api/appointments/{id}/did-not-attend` | Registrar inasistencia |
| PATCH | `/api/appointments/{id}/cancel` | Cancelar cita |

Filtros disponibles:

```text
doctorId
doctorFirstName
doctorLastName

patientId
patientFirstName
patientLastName

specialtyId
specialtyName

startDate
endDate
status

pageNumber
pageSize
```

---

## Bloqueos de agenda

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/appointmentBlocks` | Crear bloqueo normal o forzado |
| GET | `/api/appointmentBlocks` | Consultar bloqueos mediante filtros |
| DELETE | `/api/appointmentBlocks/{id}` | Desactivar bloqueo |

Filtros:

```text
doctorId
startDate
endDate
```

---

# Ejemplo de creación de cita

```json
{
  "doctorId": 1,
  "patientId": 1,
  "date": "2026-09-01",
  "startTime": "09:00:00",
  "reason": "Consulta médica general",
  "note": "Primera consulta"
}
```

El sistema obtiene automáticamente la duración de la cita a partir del horario configurado para el médico.

---

# Ejemplo de bloqueo de agenda

```json
{
  "doctorId": 1,
  "date": "2026-09-01",
  "startTime": "10:00:00",
  "endTime": "12:00:00",
  "reason": "Reunión administrativa",
  "force": false
}
```

Para solicitar un bloqueo forzado:

```json
{
  "doctorId": 1,
  "date": "2026-09-01",
  "startTime": "10:00:00",
  "endTime": "12:00:00",
  "reason": "Emergencia",
  "force": true
}
```

La operación solo se ejecutará si el usuario posee los permisos correspondientes.

---

# Swagger / OpenAPI

La API dispone de documentación interactiva mediante Swagger.

En ejecución local:

```text
https://localhost:<PUERTO>/swagger
```

Con Docker:

```text
http://localhost:8080/swagger
```

En producción:

```text
https://<URL-DEL-SERVICIO>/swagger
```

Swagger permite:

- Consultar todos los endpoints.
- Visualizar requests y responses.
- Consultar códigos HTTP.
- Ejecutar pruebas funcionales.
- Autenticarse mediante JWT.

## Autenticación desde Swagger

Primero realizar:

```text
POST /api/auth/login
```

Copiar el token JWT recibido.

Después presionar:

```text
Authorize
```

e ingresar el token.

A partir de ese momento Swagger enviará:

```text
Authorization: Bearer <TOKEN>
```

en las peticiones protegidas.

---

# Configuración del proyecto

## Requisitos

Para ejecutar el proyecto localmente se necesita:

- .NET 10 SDK.
- SQL Server 2022 o Azure SQL Database.
- Docker Desktop opcional.
- Git.
- Entity Framework Core CLI para aplicar migraciones.

---

# Variables de entorno

Los datos sensibles no deben almacenarse dentro del repositorio.

La aplicación utiliza las siguientes variables de entorno:

| Variable | Descripción |
|---|---|
| `ConnectionStrings__DbAppointments` | Cadena de conexión a SQL Server |
| `Jwt__SecretKey` | Clave utilizada para firmar el JWT |
| `Jwt__Issuer` | Emisor del JWT |
| `Jwt__Audience` | Audiencia del JWT |
| `Jwt__ExpirationInMinutes` | Duración del token en minutos |

Ejemplo:

```text
ConnectionStrings__DbAppointments=<CONNECTION_STRING>

Jwt__SecretKey=<JWT_SECRET>

Jwt__Issuer=Gestion.Citas.API

Jwt__Audience=AppointmentPatients

Jwt__ExpirationInMinutes=60
```

Nunca se deben subir contraseñas o claves reales al repositorio.

---

# Ejecución local

## 1. Clonar el repositorio

```bash
git clone https://github.com/DavidACM28/Gestion-de-Citas-Backend.git
```

Entrar a la carpeta:

```bash
cd Gestion-de-Citas-Backend
```

---

## 2. Restaurar dependencias

```bash
dotnet restore
```

---

## 3. Configurar la conexión

En PowerShell:

```powershell
$env:ConnectionStrings__DbAppointments="Server=localhost,1501;Database=dbappointments;User Id=sa;Password=<PASSWORD>;Encrypt=False;"

$env:Jwt__SecretKey="<JWT_SECRET>"

$env:Jwt__Issuer="Gestion.Citas.API"

$env:Jwt__Audience="AppointmentPatients"

$env:Jwt__ExpirationInMinutes="60"
```

---

## 4. Aplicar migraciones

```powershell
dotnet ef database update `
  --project .\Gestion.Citas.DataAccess\Gestion.Citas.DataAccess.csproj `
  --startup-project .\Gestion.Citas.API\Gestion.Citas.API.csproj `
  --connection "<CONNECTION_STRING>"
```

Esto creará la estructura de base de datos utilizando las migraciones incluidas en el proyecto.

---

## 5. Ejecutar la aplicación

```bash
dotnet run --project Gestion.Citas.API
```

La consola mostrará la URL utilizada por ASP.NET Core.

Después ingresar a:

```text
https://localhost:<PUERTO>/swagger
```

---

# Ejecución mediante Docker

La solución incluye un `Dockerfile` multi-stage.

Primero construir la imagen:

```bash
docker build -t gestion-citas-api .
```

Después ejecutar:

```powershell
docker run --rm -p 8080:8080 `
  -e "ConnectionStrings__DbAppointments=<CONNECTION_STRING>" `
  -e "Jwt__SecretKey=<JWT_SECRET>" `
  -e "Jwt__Issuer=Gestion.Citas.API" `
  -e "Jwt__Audience=AppointmentPatients" `
  -e "Jwt__ExpirationInMinutes=60" `
  gestion-citas-api
```

Swagger estará disponible en:

```text
http://localhost:8080/swagger
```

---

# Base de datos

El proyecto utiliza Entity Framework Core con SQL Server.

Las configuraciones de las entidades se encuentran en:

```text
Gestion.Citas.DataAccess/Configurations/
```

Las migraciones se encuentran en:

```text
Gestion.Citas.DataAccess/Migrations/
```

Entre las características implementadas se encuentran:

- Relaciones mediante claves foráneas.
- Restricciones de campos.
- Índices únicos.
- Soft delete mediante el campo `Active`.
- Restricción única para los slots de citas.
- Configuración de concurrencia para citas.
- Consultas mediante LINQ.
- Operaciones asíncronas.

---

# Transacciones implementadas

Se utilizan transacciones en las operaciones que necesitan modificar múltiples registros de forma atómica.

## Creación de cita

```text
Appointment
+
AppointmentSlots
```

## Actualización de cita

```text
Eliminar slots anteriores
+
Crear slots nuevos
+
Actualizar Appointment
```

## Cancelación/finalización

```text
Actualizar estado
+
Liberar AppointmentSlots
```

## Bloqueo forzado

```text
Crear bloqueo
+
Cancelar citas afectadas
+
Eliminar slots
```

El objetivo es garantizar que una operación se complete totalmente o no se aplique ninguno de sus cambios.

---

# Manejo de errores

La API incluye un middleware global para capturar excepciones inesperadas.

El sistema utiliza códigos HTTP según el resultado de las operaciones, entre ellos:

```text
200 OK
201 Created
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
500 Internal Server Error
```

Los endpoints también retornan respuestas estructuradas para indicar si una operación fue exitosa o falló y proporcionar el mensaje correspondiente.

---

# Pruebas funcionales recomendadas

Para verificar las funcionalidades principales desde Swagger se puede utilizar el siguiente flujo:

```text
1. Iniciar sesión y obtener JWT
        ↓
2. Consultar especialidades
        ↓
3. Consultar médicos
        ↓
4. Consultar horario del médico
        ↓
5. Crear una cita
        ↓
6. Verificar creación de Appointment + AppointmentSlots
        ↓
7. Intentar crear una cita en el mismo horario
        ↓
8. Verificar rechazo por conflicto
        ↓
9. Crear un bloqueo en un horario libre
        ↓
10. Intentar crear un bloqueo sobre citas existentes
        ↓
11. Verificar rechazo
        ↓
12. Ejecutar bloqueo forzado con usuario autorizado
        ↓
13. Verificar cancelación de citas y liberación de slots
```

También se deben verificar los permisos utilizando diferentes roles para comprobar que los usuarios no puedan acceder a operaciones que no les corresponden.

---

# Despliegue

La aplicación está preparada para ejecutarse mediante Docker.

Arquitectura de producción prevista:

```text
GitHub
   │
   ↓
Render
Docker / ASP.NET Core
   │
   ↓
Azure SQL Database
```

Las credenciales y cadenas de conexión deben configurarse como variables de entorno de la plataforma y nunca almacenarse en el repositorio.

## API desplegada

```text
https://gestion-citas-backend-dwqf.onrender.com
```

## Swagger desplegado

```text
https://gestion-citas-backend-dwqf.onrender.com/swagger/index.html
```

---

# Características principales implementadas

- ASP.NET Core Minimal APIs.
- .NET 10.
- Arquitectura separada por responsabilidades.
- Dependency Injection.
- Entity Framework Core.
- SQL Server.
- Migraciones.
- JWT Authentication.
- Autorización mediante roles.
- Claims.
- BCrypt para almacenamiento seguro de contraseñas.
- DTOs.
- Repository Pattern.
- Services.
- LINQ.
- Async/Await.
- Swagger/OpenAPI.
- Manejo global de excepciones.
- Slots de citas.
- Prevención de doble reserva.
- Validación de horarios.
- Bloqueos de agenda.
- Bloqueos forzados.
- Maestro-detalle.
- Transacciones.
- Índices únicos.
- Paginación y filtros.
- Docker.

---

# Objetivo técnico

Además de las operaciones CRUD requeridas para las entidades principales, el proyecto busca demostrar un proceso de negocio completo relacionado con la gestión de citas médicas.

El núcleo de la solución puede resumirse como:

```text
BusinessHours
      +
AppointmentBlock
      +
AppointmentSlot
      ↓
Disponibilidad válida del médico
```

La combinación de estas entidades permite controlar que una cita:

- Pertenezca al horario del médico.
- No colisione con bloqueos.
- No comparta slots con otra cita.
- Respete las reglas de negocio y permisos establecidos.

La operación `Appointment → AppointmentSlot` constituye el proceso maestro-detalle principal de la solución y utiliza transacciones para garantizar la integridad y consistencia de los datos.