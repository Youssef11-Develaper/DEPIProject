using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Mawidy.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        if (await context.Courts.AnyAsync())
        {
            return; // Already seeded
        }

        var courts = new List<Court>
        {
            new() { Name = "محكمة القاهرة الابتدائية", Type = CourtType.Civil, Address = "باب الخلق، القاهرة", Phone = "02-25940255", TotalRooms = 12 },
            new() { Name = "محكمة الأسرة — العباسية", Type = CourtType.Family, Address = "العباسية، القاهرة", Phone = "02-24829100", TotalRooms = 6 },
            new() { Name = "محكمة الجيزة الابتدائية", Type = CourtType.Civil, Address = "الجيزة — ميدان الجيزة", Phone = "02-35729800", TotalRooms = 10 },
            new() { Name = "محكمة استئناف القاهرة", Type = CourtType.Appeal, Address = "باب الخلق، القاهرة", Phone = "02-25910022", TotalRooms = 8 },
            new() { Name = "المحكمة الاقتصادية — القاهرة", Type = CourtType.Commercial, Address = "مدينة نصر، القاهرة", Phone = "02-24010999", TotalRooms = 4 },
            new() { Name = "محكمة الجنايات — القاهرة", Type = CourtType.Criminal, Address = "باب الخلق، القاهرة", Phone = "02-25940255", TotalRooms = 7 },
            new() { Name = "محكمة شمال القاهرة الابتدائية", Type = CourtType.Civil, Address = "مصر الجديدة، القاهرة", Phone = "02-24184900", TotalRooms = 9 },
            new() { Name = "محكمة الأسرة — مدينة نصر", Type = CourtType.Family, Address = "مدينة نصر، القاهرة", Phone = "02-24011800", TotalRooms = 5 },
        };

        context.Courts.AddRange(courts);
        await context.SaveChangesAsync(default);

        // Seed Legal Cases
        var cairoCourt = courts[0];
        var familyCourt = courts[1];

        var cases = new List<LegalCase>
        {
            new()
            {
                CaseNumber = "2025/4821",
                Year = 2025,
                CourtId = cairoCourt.Id,
                Type = "دعوى مدنية — محكمة القاهرة الابتدائية — دائرة مدنية أولى",
                Status = "نشط",
                TimelineEvents = new List<CaseTimelineEvent>
                {
                    new() { Status = TimelineEventStatus.Done, Title = "إيداع الدعوى", EventDate = new DateTime(2025, 10, 10), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "قيد القضية بالجدول", EventDate = new DateTime(2025, 10, 12), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "الجلسة الأولى — تأجيل للإعلان", EventDate = new DateTime(2025, 11, 5), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "الجلسة الثانية — تقديم مستندات", EventDate = new DateTime(2025, 12, 10), Note = "" },
                    new() { Status = TimelineEventStatus.Active, Title = "الجلسة القادمة — مرافعة", EventDate = new DateTime(2026, 1, 20), Note = "الجلسة القادمة بعد 5 أيام — احضر قبل الموعد بـ 15 دقيقة" },
                    new() { Status = TimelineEventStatus.Pending, Title = "صدور الحكم المتوقع", EventDate = new DateTime(2026, 2, 1), Note = "" }
                }
            },
            new()
            {
                CaseNumber = "2024/1190",
                Year = 2024,
                CourtId = familyCourt.Id,
                Type = "دعوى أسرة — محكمة الأسرة بالعباسية",
                Status = "حكم صدر",
                TimelineEvents = new List<CaseTimelineEvent>
                {
                    new() { Status = TimelineEventStatus.Done, Title = "إيداع الدعوى", EventDate = new DateTime(2024, 3, 15), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "جلسة التوفيق والوساطة", EventDate = new DateTime(2024, 4, 10), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "الجلسة الأولى — نظر في الموضوع", EventDate = new DateTime(2024, 5, 20), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "الجلسة الثانية — تقديم دفوع", EventDate = new DateTime(2024, 7, 15), Note = "" },
                    new() { Status = TimelineEventStatus.Done, Title = "الجلسة الثالثة — مرافعة ختامية", EventDate = new DateTime(2024, 9, 20), Note = "" },
                    new() { Status = TimelineEventStatus.Active, Title = "صدور الحكم", EventDate = new DateTime(2026, 1, 1), Note = "✅ صدر الحكم — يمكنك استخراج نسخة رسمية الآن" }
                }
            }
        };

        context.LegalCases.AddRange(cases);
        await context.SaveChangesAsync(default);
    }
}
