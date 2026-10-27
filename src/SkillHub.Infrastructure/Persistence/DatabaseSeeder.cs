using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public const string InstructorPassword = "Instructor2026*";
    public const string ParticipantPassword = "Skillhub2026*";

    private static readonly InstructorSeed[] Instructors =
    [
        new("Andrés Felipe Mejía Arango", "andres.mejia@correo.com", "Fotografía"),
        new("Rosa Elena Barrios Polo", "rosa.barrios@correo.com", "Cocina"),
        new("Mauricio Jiménez Cárdenas", "mauricio.jimenez@correo.com", "Oratoria"),
        new("Natalia Herrera Castaño", "natalia.herrera@correo.com", "Diseño gráfico"),
        new("Hernán Darío Quintero Gil", "hernan.quintero@correo.com", "Música"),
        new("Paola Andrea Vargas Mora", "paola.vargas@correo.com", "Marketing digital")
    ];

    private static readonly ParticipantSeed[] Participants =
    [
        new("Jorge Andrés Pérez Rojas", "jorge.perez@correo.com"),
        new("Luz Marina Ospina Ríos", "luz.ospina@correo.com"),
        new("Carlos Eduardo Rincón Díaz", "carlos.rincon@correo.com"),
        new("Valentina Gómez Ruiz", "valentina.gomez@correo.com"),
        new("Juan Camilo Restrepo Vélez", "juan.restrepo@correo.com"),
        new("María Fernanda Torres León", "maria.torres@correo.com"),
        new("Santiago Ramírez Henao", "santiago.ramirez@correo.com"),
        new("Daniela Morales Pineda", "daniela.morales@correo.com"),
        new("Sebastián Castro Duque", "sebastian.castro@correo.com"),
        new("Laura Sofía Martínez Suárez", "laura.martinez@correo.com"),
        new("Andrés Mauricio Salazar Ortiz", "andres.salazar@correo.com"),
        new("Camila Andrea Cárdenas Ruiz", "camila.cardenas@correo.com"),
        new("Diego Alejandro Muñoz Peña", "diego.munoz@correo.com"),
        new("Juliana Ortega Sánchez", "juliana.ortega@correo.com"),
        new("Felipe Agudelo Marín", "felipe.agudelo@correo.com"),
        new("Natalia Montoya Correa", "natalia.montoya@correo.com"),
        new("Óscar Iván Guerrero Lozano", "oscar.guerrero@correo.com"),
        new("Paula Andrea Zapata Ríos", "paula.zapata@correo.com"),
        new("Cristian David Becerra Mejía", "cristian.becerra@correo.com"),
        new("Mariana Londoño Álvarez", "mariana.londono@correo.com"),
        new("Javier Alonso Cuéllar Pardo", "javier.cuellar@correo.com"),
        new("Isabella Navarro Fuentes", "isabella.navarro@correo.com"),
        new("Kevin Stiven Mosquera Palacios", "kevin.mosquera@correo.com"),
        new("Ana María Jaramillo Gaviria", "ana.jaramillo@correo.com"),
        new("Luis Fernando Arias Cano", "luis.arias@correo.com"),
        new("Sara Valencia Bermúdez", "sara.valencia@correo.com"),
        new("Esteban Rodríguez Molina", "esteban.rodriguez@correo.com"),
        new("Manuela Escobar Toro", "manuela.escobar@correo.com"),
        new("Wilmer Alfonso Benítez Cruz", "wilmer.benitez@correo.com"),
        new("Yuliana Patiño Osorio", "yuliana.patino@correo.com")
    ];

    private static readonly WorkshopSeed[] Workshops =
    [
        new("FOT-001", "Fotografía con celular", "Composición, luz y edición básica desde el celular", "andres.mejia@correo.com", new DateTime(2026, 12, 5, 13, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 5, 17, 0, 0, DateTimeKind.Utc), 15),
        new("FOT-002", "Retrato en exteriores", "Manejo de luz natural para retratos al aire libre", "andres.mejia@correo.com", new DateTime(2027, 1, 16, 13, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 16, 17, 0, 0, DateTimeKind.Utc), 10),
        new("FOT-003", "Edición en Lightroom", "Flujo de revelado y ajustes de color", "andres.mejia@correo.com", new DateTime(2027, 2, 6, 19, 0, 0, DateTimeKind.Utc), new DateTime(2027, 2, 6, 23, 0, 0, DateTimeKind.Utc), 12),
        new("COC-001", "Cocina costeña", "Arroz con coco, patacones y mojarra frita", "rosa.barrios@correo.com", new DateTime(2026, 12, 5, 19, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 5, 23, 0, 0, DateTimeKind.Utc), 12),
        new("COC-002", "Repostería básica", "Tortas, galletas y postres de nevera", "rosa.barrios@correo.com", new DateTime(2027, 1, 9, 14, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 9, 18, 0, 0, DateTimeKind.Utc), 10),
        new("COC-003", "Cocina saludable", "Platos balanceados para la semana", "rosa.barrios@correo.com", new DateTime(2027, 2, 13, 14, 0, 0, DateTimeKind.Utc), new DateTime(2027, 2, 13, 17, 0, 0, DateTimeKind.Utc), 14),
        new("ORA-001", "Oratoria para principiantes", "Técnicas para hablar en público con seguridad", "mauricio.jimenez@correo.com", new DateTime(2026, 12, 12, 14, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 12, 17, 0, 0, DateTimeKind.Utc), 20),
        new("ORA-002", "Presentaciones efectivas", "Cómo estructurar y presentar una idea en 10 minutos", "mauricio.jimenez@correo.com", new DateTime(2027, 1, 23, 14, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 23, 17, 0, 0, DateTimeKind.Utc), 18),
        new("DIS-001", "Diseño en Canva", "Piezas gráficas para redes sociales", "natalia.herrera@correo.com", new DateTime(2026, 12, 19, 19, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 19, 22, 0, 0, DateTimeKind.Utc), 18),
        new("DIS-002", "Fundamentos de color y tipografía", "Principios visuales para piezas profesionales", "natalia.herrera@correo.com", new DateTime(2027, 1, 30, 19, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 30, 22, 0, 0, DateTimeKind.Utc), 15),
        new("MUS-001", "Guitarra desde cero", "Acordes básicos y primeras canciones", "hernan.quintero@correo.com", new DateTime(2026, 12, 12, 19, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 12, 22, 0, 0, DateTimeKind.Utc), 10),
        new("MUS-002", "Producción musical en casa", "Grabación y mezcla con herramientas gratuitas", "hernan.quintero@correo.com", new DateTime(2027, 2, 20, 19, 0, 0, DateTimeKind.Utc), new DateTime(2027, 2, 20, 23, 0, 0, DateTimeKind.Utc), 12),
        new("MKT-001", "Marketing en redes sociales", "Estrategia de contenido para pequeños negocios", "paola.vargas@correo.com", new DateTime(2026, 12, 19, 13, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 19, 17, 0, 0, DateTimeKind.Utc), 20),
        new("MKT-002", "Publicidad en Meta e Instagram", "Configuración y medición de campañas", "paola.vargas@correo.com", new DateTime(2027, 1, 16, 19, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 16, 23, 0, 0, DateTimeKind.Utc), 16),
        new("MKT-003", "Emprendimiento digital", "Validar y lanzar una idea de negocio en línea", "paola.vargas@correo.com", new DateTime(2027, 2, 27, 14, 0, 0, DateTimeKind.Utc), new DateTime(2027, 2, 27, 17, 0, 0, DateTimeKind.Utc), 20)
    ];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<SkillHubDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { Roles.Employee, Roles.Participant })
        {
            if (!await roleManager.RoleExistsAsync(role))
                EnsureSucceeded(await roleManager.CreateAsync(new IdentityRole<Guid>(role)), $"Could not create role {role}.");
        }

        foreach (var seed in Instructors)
        {
            var user = await EnsureUserAsync(userManager, seed.Email, seed.FullName, InstructorPassword, Roles.Employee);
            var exists = await db.Instructors.AnyAsync(instructor => instructor.Email == seed.Email);
            if (!exists)
            {
                db.Instructors.Add(new Instructor(user.Id, seed.FullName, seed.Email, seed.Specialty));
                await db.SaveChangesAsync();
            }
        }

        foreach (var seed in Participants)
        {
            var user = await EnsureUserAsync(userManager, seed.Email, seed.FullName, ParticipantPassword, Roles.Participant);
            var exists = await db.Participants.AnyAsync(participant => participant.Email == seed.Email);
            if (!exists)
            {
                db.Participants.Add(new Participant(user.Id, seed.FullName, seed.Email));
                await db.SaveChangesAsync();
            }
        }

        var instructors = await db.Instructors.ToDictionaryAsync(instructor => instructor.Email);
        var now = DateTime.UtcNow;

        foreach (var seed in Workshops)
        {
            if (await db.Workshops.AnyAsync(workshop => workshop.Code == seed.Code)) continue;
            if (!instructors.TryGetValue(seed.InstructorEmail, out var instructor)) continue;

            db.Workshops.Add(new Workshop(
                seed.Code,
                seed.Title,
                seed.Description,
                instructor.Id,
                seed.StartAt,
                seed.EndAt,
                seed.Capacity,
                instructor.UserId,
                now));
        }

        await db.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string fullName,
        string password,
        string role)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = normalizedEmail,
                Email = normalizedEmail,
                FullName = fullName,
                EmailConfirmed = true
            };
            EnsureSucceeded(await userManager.CreateAsync(user, password), $"Could not create user {normalizedEmail}.");
        }

        if (!await userManager.IsInRoleAsync(user, role))
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role), $"Could not assign role {role} to {normalizedEmail}.");

        return user;
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded) return;
        throw new InvalidOperationException(
            $"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }

    private sealed record InstructorSeed(string FullName, string Email, string Specialty);
    private sealed record ParticipantSeed(string FullName, string Email);
    private sealed record WorkshopSeed(
        string Code,
        string Title,
        string Description,
        string InstructorEmail,
        DateTime StartAt,
        DateTime EndAt,
        int Capacity);
}
