using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;

namespace SkillHub.Infrastructure.Pdf;

public sealed class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    private static readonly TimeSpan ColombiaOffset = TimeSpan.FromHours(-5);

    public QuestPdfCertificateGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(CertificateData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(34);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(style => style.FontSize(11).FontColor("#1f2937"));

                page.Content()
                    .Border(2)
                    .BorderColor("#0f766e")
                    .Padding(30)
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(brand =>
                            {
                                brand.Item().Text("SKILLHUB").FontSize(30).Bold().FontColor("#0f766e");
                                brand.Item().Text("Centro de formación · Talleres presenciales")
                                    .FontSize(10).FontColor("#64748b");
                            });
                            row.ConstantItem(170).AlignRight().Column(reference =>
                            {
                                reference.Item().Text("CONSTANCIA")
                                    .FontSize(9).Bold().FontColor("#94a3b8");
                                reference.Item().Text(data.WorkshopCode)
                                    .FontSize(18).Bold().FontColor("#0f172a");
                            });
                        });

                        column.Item().PaddingTop(18).LineHorizontal(1).LineColor("#e2e8f0");

                        column.Item().PaddingTop(30).AlignCenter()
                            .Text("Constancia de inscripción")
                            .FontSize(32).Bold().FontColor("#0f172a");

                        column.Item().PaddingTop(10).AlignCenter()
                            .Text("La presente constancia certifica que")
                            .FontSize(12).FontColor("#64748b");

                        column.Item().PaddingTop(16).AlignCenter()
                            .Text(data.ParticipantName)
                            .FontSize(24).Bold().FontColor("#0f766e");

                        column.Item().PaddingTop(2).AlignCenter()
                            .Text(data.ParticipantEmail)
                            .FontSize(11).FontColor("#64748b");

                        column.Item().PaddingTop(14).AlignCenter()
                            .Text("se encuentra inscrito(a) en el taller")
                            .FontSize(12).FontColor("#64748b");

                        column.Item().PaddingTop(8).AlignCenter()
                            .Text($"{data.WorkshopTitle} ({data.WorkshopCode})")
                            .FontSize(18).SemiBold().FontColor("#0f172a");

                        column.Item().PaddingTop(6).AlignCenter()
                            .Text($"Instructor: {data.InstructorName}")
                            .FontSize(11);

                        column.Item().PaddingTop(3).AlignCenter()
                            .Text($"Horario: {Format(data.StartAt)} — {Format(data.EndAt)}")
                            .FontSize(11);

                        column.Item().PaddingTop(34).Row(row =>
                        {
                            row.RelativeItem().Column(item =>
                            {
                                item.Item().Text("FECHA DE INSCRIPCIÓN")
                                    .FontSize(8).Bold().FontColor("#94a3b8");
                                item.Item().Text(FormatDate(data.EnrolledAt)).FontSize(11);
                            });

                            row.RelativeItem().AlignCenter().Column(item =>
                            {
                                item.Item().Text("CÓDIGO DEL TALLER")
                                    .FontSize(8).Bold().FontColor("#94a3b8");
                                item.Item().Text(data.WorkshopCode).FontSize(11);
                            });

                            row.RelativeItem().AlignRight().Column(item =>
                            {
                                item.Item().Text("FECHA DE GENERACIÓN")
                                    .FontSize(8).Bold().FontColor("#94a3b8");
                                item.Item().Text(FormatDate(data.GeneratedAt)).FontSize(11);
                            });
                        });

                        column.Item().PaddingTop(26).AlignCenter()
                            .Text("Documento generado automáticamente por SkillHub.")
                            .FontSize(8).FontColor("#94a3b8");
                    });
            });
        }).GeneratePdf();
    }

    private static DateTime ToColombia(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return utc.Add(ColombiaOffset);
    }

    private static string Format(DateTime value) =>
        ToColombia(value).ToString("dd/MM/yyyy 'de' HH:mm");

    private static string FormatDate(DateTime value) =>
        ToColombia(value).ToString("dd/MM/yyyy HH:mm");
}
