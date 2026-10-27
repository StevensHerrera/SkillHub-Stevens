using System.Globalization;
using ClosedXML.Excel;
using SkillHub.Application.Common.Exceptions;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.Common.Models;

namespace SkillHub.Infrastructure.Excel;

public sealed class ClosedXmlEnrollmentParser : IEnrollmentExcelParser
{
    private const string ParticipantHeader = "participantemail";
    private const string WorkshopHeader = "workshopcode";

    public IReadOnlyList<ImportRow> Parse(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);

        foreach (var sheet in workbook.Worksheets)
        {
            var headerRow = sheet.FirstRowUsed();
            if (headerRow is null) continue;

            var columns = headerRow.CellsUsed()
                .Select(cell => new
                {
                    Index = cell.Address.ColumnNumber,
                    Name = cell.GetString().Trim().Replace(" ", string.Empty).ToLowerInvariant()
                })
                .ToList();

            var participantColumn = columns.FirstOrDefault(column => column.Name == ParticipantHeader)?.Index;
            var workshopColumn = columns.FirstOrDefault(column => column.Name == WorkshopHeader)?.Index;
            if (participantColumn is null || workshopColumn is null) continue;

            return ReadRows(sheet, headerRow.RowNumber(), participantColumn.Value, workshopColumn.Value);
        }

        throw new BadRequestException(
            "The Excel file must contain a sheet with the columns 'ParticipantEmail' and 'WorkshopCode'.");
    }

    private static IReadOnlyList<ImportRow> ReadRows(
        IXLWorksheet sheet,
        int headerRow,
        int participantColumn,
        int workshopColumn)
    {
        var firstDataRow = headerRow + 1;
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? headerRow;
        var rows = new List<ImportRow>();

        for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
        {
            var participantEmail = sheet.Cell(rowNumber, participantColumn).GetFormattedString().Trim();
            var workshopCode = sheet.Cell(rowNumber, workshopColumn).GetFormattedString().Trim();

            if (participantEmail.Length == 0 && workshopCode.Length == 0) continue;

            rows.Add(new ImportRow(
                rowNumber,
                participantEmail.ToLowerInvariant(),
                workshopCode.ToUpper(CultureInfo.InvariantCulture)));
        }

        return rows;
    }
}
