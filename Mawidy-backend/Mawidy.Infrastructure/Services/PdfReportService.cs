using Mawidy.Application.Interfaces;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Mawidy.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        private readonly AppDbContext _context;

        public PdfReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenerateDailyReportAsync(DateTime date, int? branchId = null)
        {
            var query = _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Branch)
                .Include(a => a.ServiceType)
                .Where(a => a.AppointmentDate.Date == date.Date);

            if (branchId.HasValue)
                query = query.Where(a => a.BranchId == branchId);

            var appointments = await query
                .OrderBy(a => a.Branch.Name)
                .ThenBy(a => a.TimeSlot)
                .ToListAsync();

            var total = appointments.Count;
            var completed = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            var cancelled = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
            var pending = appointments.Count(a => a.Status == AppointmentStatus.Pending
                || a.Status == AppointmentStatus.Confirmed);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("??????? ??????").FontSize(20).Bold();
                                c.Item().Text("????? ?????? ??????")
                                    .FontSize(12).FontColor(Colors.Grey.Darken1);
                            });

                            row.ConstantItem(150).AlignRight().Column(c =>
                            {
                                c.Item().Text($"???????: {date:dd/MM/yyyy}").FontSize(11);
                            });
                        });

                        col.Item().PaddingTop(10)
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(15).Column(col =>
                    {
                        // Stats
                        col.Item().Row(row =>
                        {
                            StatBox(row, "????????", total.ToString(), Colors.Blue.Medium);
                            StatBox(row, "??????", completed.ToString(), Colors.Green.Medium);
                            StatBox(row, "?????", cancelled.ToString(), Colors.Red.Medium);
                            StatBox(row, "?????", pending.ToString(), Colors.Orange.Medium);
                        });

                        col.Item().PaddingTop(20);

                        if (!appointments.Any())
                        {
                            col.Item().PaddingTop(30).AlignCenter()
                                .Text("???? ?????? ?? ????? ??")
                                .FontSize(14).FontColor(Colors.Grey.Medium);
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    HeaderCell(header, "???????");
                                    HeaderCell(header, "??????");
                                    HeaderCell(header, "?????");
                                    HeaderCell(header, "?????");
                                    HeaderCell(header, "??????");
                                });

                                foreach (var appointment in appointments)
                                {
                                    BodyCell(table, appointment.User.FullName);
                                    BodyCell(table, appointment.ServiceType.Name);
                                    BodyCell(table, appointment.Branch.Name);
                                    BodyCell(table, appointment.TimeSlot.ToString(@"hh\:mm"));
                                    BodyCell(table, GetStatusText(appointment.Status));
                                }
                            });
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("?? ????? ??????? ?? ").FontSize(9).FontColor(Colors.Grey.Medium);
                        text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void StatBox(RowDescriptor row, string label, string value, string color)
        {
            row.RelativeItem()
                .Border(1).BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten4)
                .Padding(10).Column(col =>
                {
                    col.Item().AlignCenter().Text(value).FontSize(22).Bold().FontColor(color);
                    col.Item().AlignCenter().Text(label).FontSize(10).FontColor(Colors.Grey.Darken1);
                });
        }

        private void HeaderCell(TableCellDescriptor header, string text)
        {
            header.Cell().Background(Colors.Blue.Darken2)
                .Padding(6).Text(text).FontColor(Colors.White).Bold().FontSize(10);
        }

        private void BodyCell(TableDescriptor table, string text)
        {
            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(6).Text(text).FontSize(10);
        }

        private string GetStatusText(AppointmentStatus status) => status switch
        {
            AppointmentStatus.Pending => "????",
            AppointmentStatus.Confirmed => "????",
            AppointmentStatus.Completed => "?????",
            AppointmentStatus.Cancelled => "????",
            _ => ""
        };
    }
}

