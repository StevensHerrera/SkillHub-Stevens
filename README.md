# SkillHub

SkillHub is a workshop management platform for a training center that offers short,
on-site workshops (photography, cooking, public speaking, design, music and digital
marketing). Participants can browse the catalog, enroll in workshops, review their
enrollments, cancel them and download an enrollment certificate in PDF. SkillHub
employees publish workshops, review the participants of any workshop, import mass
enrollments from Excel and consult the most popular workshops.

The solution is composed of a REST API built with ASP.NET Core and a participant web
application built with Angular. Both are part of the same repository.

## Technologies

| Layer | Technology |
|---|---|
| Domain | .NET 10 class library, entities and business rules |
| Application | .NET 10 class library, use cases, DTOs and abstractions |
| Infrastructure | EF Core 10 (Code First + migrations), ASP.NET Core Identity, JWT, ClosedXML, QuestPDF |
| API | ASP.NET Core 10 Web API, Swagger, JWT bearer authentication |
| Web | Angular 22 standalone components, reactive forms, HTTP interceptor |
| Database | SQLite (file based, no external server required) |
| Tests | xUnit |

No Docker configuration is included.

## Architecture

The solution follows Clean Architecture. Dependencies point toward the domain.

```
SkillHub.sln
src/
  SkillHub.Domain/          Entities, enums, roles and business rules
  SkillHub.Application/     Use cases, DTOs, interfaces and application services
  SkillHub.Infrastructure/  EF Core, repositories, Identity, JWT, Excel, PDF, seeding
  SkillHub.Api/             Controllers, middleware, authentication and Swagger
  SkillHub.Web/             Angular participant application (option B)
tests/
  SkillHub.Tests/           Unit tests for domain rules
database/
  SkillHub.sql              SQLite script with schema and data
```

Key patterns:

- Dependency injection through the native .NET container.
- Repository and Unit of Work abstractions defined in the Application layer.
- DTOs for every API request and response.
- Business rules enforced in the Domain and Application layers, never in controllers.
- `ICertificatePdfGenerator` and `IEnrollmentExcelParser` are abstractions declared in
  Application and implemented in Infrastructure.

## Requirements

- .NET SDK 10.0 or later.
- Node.js 22 or later and npm (only for the Angular application).
- No database server is required. The API creates `skillhub.db` automatically.

## Getting started

### 1. Backend

From the repository root:

```bash
dotnet restore SkillHub.sln
dotnet tool restore
dotnet run --project src/SkillHub.Api
```

The API listens on `http://localhost:5080` and Swagger is available at
`http://localhost:5080/swagger`.

On startup the API applies the EF Core migrations and seeds the data provided in
`SkillHub_Data.xlsx`: instructors, participants and workshops. Seeding is idempotent.
The `Enrollments` sheet is **not** seeded; it must be loaded through the import endpoint.

The development JWT key is defined in `src/SkillHub.Api/appsettings.Development.json`.
For other environments, provide a secret of at least 32 bytes:

```bash
export Jwt__Key="replace-with-a-secret-of-at-least-32-bytes"
```

Optional: generate the database manually before the first run:

```bash
dotnet ef database update \
  --project src/SkillHub.Infrastructure \
  --startup-project src/SkillHub.Api
```

### 2. Participant web application

In a second terminal:

```bash
cd SkillHub.Web
npm install
npm start
```

Open `http://localhost:4200`. The Angular application consumes the REST API with JWT.
The API URL is configured in `src/environments/environment.ts`
(`http://localhost:5080/api/v1`).

### 3. Tests

```bash
dotnet test SkillHub.sln
```

## Employee credentials

Every instructor is an employee account. The password is the same for all seeded
employees.

| Full name | Email | Specialty | Password |
|---|---|---|---|
| Andrés Felipe Mejía Arango | andres.mejia@correo.com | Fotografía | `Instructor2026*` |
| Rosa Elena Barrios Polo | rosa.barrios@correo.com | Cocina | `Instructor2026*` |
| Mauricio Jiménez Cárdenas | mauricio.jimenez@correo.com | Oratoria | `Instructor2026*` |
| Natalia Herrera Castaño | natalia.herrera@correo.com | Diseño gráfico | `Instructor2026*` |
| Hernán Darío Quintero Gil | hernan.quintero@correo.com | Música | `Instructor2026*` |
| Paola Andrea Vargas Mora | paola.vargas@correo.com | Marketing digital | `Instructor2026*` |

Participant accounts are also seeded with the password `Skillhub2026*`, for example
`jorge.perez@correo.com` or `laura.martinez@correo.com`. New users always register as
participants.

Employee operations (manage workshops, instructors and participants, list workshop
enrollments, import Excel and consult reports) are demonstrated directly in
Swagger/Postman, as requested. The Angular application covers the participant flow.

When an employee creates an instructor or a participant without a password, the
seeded default password is used (`Instructor2026*` and `Skillhub2026*`
respectively).

## API endpoints

Base path: `/api/v1`

| Method | Route | Access | Description |
|---|---|---|---|
| `POST` | `/auth/register` | Public | Register a new participant |
| `POST` | `/auth/login` | Public | Sign in and return a JWT |
| `GET` | `/workshops` | Public | Upcoming workshops with available seats |
| `GET` | `/workshops/{id}` | Public | Workshop detail with available seats |
| `POST` | `/workshops` | Employee | Create a workshop |
| `PUT` | `/workshops/{id}` | Employee | Update a workshop |
| `GET` | `/workshops/{id}/enrollments` | Employee | List the enrollments of a workshop |
| `GET` | `/instructors` | Employee | List instructors (employees) |
| `GET` | `/instructors/{id}` | Employee | Instructor detail |
| `POST` | `/instructors` | Employee | Create an instructor (employee) |
| `PUT` | `/instructors/{id}` | Employee | Update an instructor |
| `GET` | `/participants` | Employee | List participants |
| `GET` | `/participants/{id}` | Employee | Participant detail |
| `POST` | `/participants` | Employee | Create a participant |
| `PUT` | `/participants/{id}` | Employee | Update a participant |
| `POST` | `/enrollments` | Participant | Enroll the authenticated participant |
| `GET` | `/enrollments/mine` | Participant | List the authenticated participant's enrollments |
| `PATCH` | `/enrollments/{id}/cancel` | Owner or employee | Cancel an enrollment |
| `POST` | `/enrollments/import` | Employee | Import enrollments from an Excel file |
| `GET` | `/enrollments/{id}/certificate` | Owner or employee | Download the enrollment certificate (PDF) |
| `GET` | `/reports/top-workshops` | Employee | Workshops ordered by active enrollments |

Swagger includes a **Bearer** security definition. Call `/auth/login`, copy the
`accessToken` value and paste it into the **Authorize** dialog.

### Example: employee login and Excel import

```bash
TOKEN=$(curl -s -X POST http://localhost:5080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"andres.mejia@correo.com","password":"Instructor2026*"}' \
  | jq -r .accessToken)

curl -X POST http://localhost:5080/api/v1/enrollments/import \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@SkillHub_Data.xlsx"
```

The import response contains `rowsRead`, `enrolled`, `failed` and an `errors` array
with the row number and reason for every rejected row. Valid rows are persisted even
when other rows fail.

### Example: create a workshop

```bash
curl -X POST http://localhost:5080/api/v1/workshops \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "FOT-100",
    "title": "Fotografía nocturna",
    "description": "Uso de luz artificial y larga exposición",
    "instructorEmail": "andres.mejia@correo.com",
    "startAt": "2027-03-10T18:00:00-05:00",
    "endAt": "2027-03-10T22:00:00-05:00",
    "capacity": 12
  }'
```

### Example: create an instructor and a participant

```bash
curl -X POST http://localhost:5080/api/v1/instructors \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Nuevo Instructor","email":"nuevo.instructor@correo.com","specialty":"Fotografía"}'

curl -X POST http://localhost:5080/api/v1/participants \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Nuevo Participante","email":"nuevo.participante@correo.com"}'
```

If `password` is omitted, the default employee or participant password is applied.
Updating an instructor or participant also updates the related Identity account, so the
new email can be used to sign in.

## Business rules

- Any visitor can register, but registration always creates a **Participant**.
- Only employees can manage workshops, instructors and participants.
- Only employees can create workshops. The instructor is resolved by email and must
  exist in the platform.
- A workshop is created with a future start date and an end date after the start date.
- A workshop can be updated while its start date remains in the future. Its code must
  stay unique and its capacity cannot be lower than its current active enrollments.
- Changing a workshop schedule is rejected when it would overlap another active
  enrollment of a participant already enrolled in it.
- The public catalog only shows workshops that have not started, with server-calculated
  available seats.
- An enrollment is born **Active** and can become **Cancelled**.
- An enrollment is rejected when the workshop is full, when the participant already has
  an active enrollment in the same workshop, when the schedule overlaps another active
  enrollment, or when the workshop has already started.
- Participants can cancel up to 24 hours before the workshop starts. Employees can
  cancel any enrollment while the workshop has not started.
- Every cancellation stores who performed it and when.
- A participant who cancelled can enroll again if seats and rules still allow it.
- Certificates are only generated for active enrollments, and only for the owner or an
  employee.
- The top-workshops report orders workshops by active enrollment count.

Times are stored in UTC and displayed in the participant's local time (the seeded
schedule is expressed in Colombia time, UTC-05:00).

## Excel import format

The parser looks for a worksheet containing these headers (case-insensitive, spaces are
ignored):

| ParticipantEmail | WorkshopCode |
|---|---|
| jorge.perez@correo.com | COC-001 |

If the participant or the workshop does not exist, the row is rejected. Each accepted
row is processed as a normal enrollment and must satisfy all business rules.

## Enrollment certificate

The certificate is generated in the Infrastructure layer through QuestPDF, behind the
`ICertificatePdfGenerator` abstraction declared in Application. It contains the
SkillHub name, the title "Constancia de inscripción", participant name and email,
workshop code, title, instructor, date and time, enrollment date and generation date.

## Database script

`database/SkillHub.sql` is a SQLite script generated from the final database. It
contains the schema created by the EF Core migrations plus the seeded instructors,
participants, workshops and the enrollments loaded through the import endpoint.

To recreate the database from the script:

```bash
sqlite3 skillhub.db < database/SkillHub.sql
```

## Author

- **Full name:** Stevens Andres Herrera Hernandez
- **Email:** steveherrera98@hotmail.com
