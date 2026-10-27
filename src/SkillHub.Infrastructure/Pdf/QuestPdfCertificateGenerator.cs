using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;

namespace SkillHub.Infrastructure.Pdf;

public sealed class QuestPdfCertificateGenerator : ICertificatePdfGenerator
{
    private const string Paper = "#F4F2EA";
    private const string Surface = "#FFFDF7";
    private const string Mustard = "#C99A06";
    private const string MustardDark = "#8A6700";
    private const string MustardSoft = "#FCF6DF";
    private const string MustardLine = "#EADFBB";
    private const string Ink = "#2C2A22";
    private const string InkSoft = "#514B39";
    private const string Muted = "#867F6A";
    private const string Line = "#E5DFCF";

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
                page.PageColor(Paper);
                page.DefaultTextStyle(style => style.FontSize(11).FontColor(Ink));

                page.Content()
                    .Background(Surface)
                    .Border(1.5f)
                    .BorderColor("#D9CFB0")
                    .Column(column =>
                    {
                        column.Item().Height(8).Background(Mustard);

                        column.Item().Padding(30).Column(content =>
                        {
                            content.Item().Row(row =>
                            {
                                row.RelativeItem().Column(brand =>
                                {
                                    brand.Item().Text("SKILLHUB")
                                        .FontSize(32).Bold().FontColor(MustardDark);
                                    brand.Item().Text("Centro de formación · Talleres presenciales")
                                        .FontSize(10).FontColor(Muted);
                                });

                                row.ConstantItem(190).AlignRight()
                                    .Background(MustardSoft)
                                    .Border(1)
                                    .BorderColor(MustardLine)
                                    .Padding(10)
                                    .Column(reference =>
                                    {
                                        reference.Item().AlignRight().Text("CONSTANCIA")
                                            .FontSize(8).Bold().FontColor(MustardDark);
                                        reference.Item().AlignRight().Text(data.WorkshopCode)
                                            .FontSize(18).Bold().FontColor(Ink);
                                    });
                            });

                            content.Item().PaddingTop(18).LineHorizontal(1).LineColor(Line);

                            content.Item().PaddingTop(28).AlignCenter()
                                .Text("Constancia de inscripción")
                                .FontSize(34).Bold().FontColor(Ink);

                            content.Item().PaddingTop(10).AlignCenter()
                                .Text("La presente constancia certifica que")
                                .FontSize(12).FontColor(Muted);

                            content.Item().PaddingTop(14).AlignCenter()
                                .Text(data.ParticipantName)
                                .FontSize(26).Bold().FontColor(Ink);

                            content.Item().PaddingTop(4).AlignCenter()
                                .Width(90).LineHorizontal(3).LineColor(Mustard);

                            content.Item().PaddingTop(3).AlignCenter()
                                .Text(data.ParticipantEmail)
                                .FontSize(11).FontColor(Muted);

                            content.Item().PaddingTop(14).AlignCenter()
                                .Text("se encuentra inscrito(a) en el taller")
                                .FontSize(12).FontColor(Muted);

                            content.Item().PaddingTop(8).AlignCenter()
                                .Text($"{data.WorkshopTitle} ({data.WorkshopCode})")
                                .FontSize(19).SemiBold().FontColor(Ink);

                            content.Item().PaddingTop(7).AlignCenter()
                                .Text($"Instructor: {data.InstructorName}")
                                .FontSize(11).FontColor(InkSoft);

                            content.Item().PaddingTop(3).AlignCenter()
                                .Text($"Horario: {Format(data.StartAt)} — {Format(data.EndAt)}")
                                .FontSize(11).FontColor(InkSoft);

                            content.Item().PaddingTop(30).Row(row =>
                            {
                                row.RelativeItem()
                                    .Background(MustardSoft)
                                    .Border(1)
                                    .BorderColor(MustardLine)
                                    .Padding(12)
                                    .Column(item =>
                                    {
                                        item.Item().Text("FECHA DE INSCRIPCIÓN")
                                            .FontSize(8).Bold().FontColor(MustardDark);
                                        item.Item().Text(FormatDate(data.EnrolledAt))
                                            .FontSize(11).FontColor(Ink);
                                    });

                                row.ConstantItem(12);

                                row.RelativeItem()
                                    .Background(MustardSoft)
                                    .Border(1)
                                    .BorderColor(MustardLine)
                                    .Padding(12)
                                    .Column(item =>
                                    {
                                        item.Item().Text("CÓDIGO DEL TALLER")
                                            .FontSize(8).Bold().FontColor(MustardDark);
                                        item.Item().Text(data.WorkshopCode)
                                            .FontSize(11).FontColor(Ink);
                                    });

                                row.ConstantItem(12);

                                row.RelativeItem()
                                    .Background(MustardSoft)
                                    .Border(1)
                                    .BorderColor(MustardLine)
                                    .Padding(12)
                                    .Column(item =>
                                    {
                                        item.Item().Text("FECHA DE GENERACIÓN")
                                            .FontSize(8).Bold().FontColor(MustardDark);
                                        item.Item().Text(FormatDate(data.GeneratedAt))
                                            .FontSize(11).FontColor(Ink);
                                    });
                            });

                            content.Item().PaddingTop(24).AlignCenter()
                                .Text("Documento generado automáticamente por SkillHub.")
                                .FontSize(8).FontColor(Muted);
                        });
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
