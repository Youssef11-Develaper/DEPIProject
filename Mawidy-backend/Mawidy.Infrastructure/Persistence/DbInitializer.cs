using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mawidy.Domain.Entities;
using Mawidy.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Mawidy.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Apply any pending migrations automatically on startup
            await context.Database.MigrateAsync();

            if (!await context.Operators.AnyAsync())
            {
                // Get Governorates to resolve IDs dynamically
                var govs = await context.Governorates.ToListAsync();
                var cairo = govs.FirstOrDefault(g => g.Name == "القاهرة") ?? govs.First();
                var giza = govs.FirstOrDefault(g => g.Name == "الجيزة") ?? govs.First();
                var alex = govs.FirstOrDefault(g => g.Name == "الإسكندرية") ?? govs.First();
                var qalyubia = govs.FirstOrDefault(g => g.Name == "القليوبية") ?? govs.First();
                var dakahlia = govs.FirstOrDefault(g => g.Name == "الدقهلية") ?? govs.First();
                var sharqia = govs.FirstOrDefault(g => g.Name == "الشرقية") ?? govs.First();
                var gharbia = govs.FirstOrDefault(g => g.Name == "الغربية") ?? govs.First();
                var beheira = govs.FirstOrDefault(g => g.Name == "البحيرة") ?? govs.First();
                var monufia = govs.FirstOrDefault(g => g.Name == "المنوفية") ?? govs.First();

                // 1. Seed Operators
                var operators = new List<Operator>
                {
                    new Operator { Key = "vodafone", NameAr = "فودافون", Color = "#E53935", BgColor = "#ffebee", Emoji = "🔴", Hotline = "19888" },
                    new Operator { Key = "orange", NameAr = "أورنج", Color = "#E65100", BgColor = "#fff3e0", Emoji = "🟠", Hotline = "16500" },
                    new Operator { Key = "etisalat", NameAr = "اتصالات مصر", Color = "#1565C0", BgColor = "#e3f2fd", Emoji = "🔵", Hotline = "19500" },
                    new Operator { Key = "we", NameAr = "WE", Color = "#2E7D32", BgColor = "#e8f5e9", Emoji = "🟢", Hotline = "15500" },
                    new Operator { Key = "civil_registry", NameAr = "السجل المدني", Color = "#0D47A1", BgColor = "#E3F2FD", Emoji = "🏛", Hotline = "153" },
                    new Operator { Key = "nbe", NameAr = "البنك الأهلي المصري", Color = "#1B5E20", BgColor = "#E8F5E9", Emoji = "💚", Hotline = "19623" },
                    new Operator { Key = "banquemisr", NameAr = "بنك مصر", Color = "#FFC107", BgColor = "#FFFDE7", Emoji = "💛", Hotline = "19888" },
                    new Operator { Key = "cib", NameAr = "البنك التجاري الدولي CIB", Color = "#0277BD", BgColor = "#E1F5FE", Emoji = "💙", Hotline = "19666" },
                    new Operator { Key = "qnb", NameAr = "بنك قطر الوطني الأهلي QNB", Color = "#311B92", BgColor = "#EDE7F6", Emoji = "💜", Hotline = "19700" },
                    new Operator { Key = "alexbank", NameAr = "بنك الإسكندرية", Color = "#E65100", BgColor = "#FFF3E0", Emoji = "🧡", Hotline = "19033" },
                    new Operator { Key = "hospital_generic", NameAr = "المستشفيات والعيادات", Color = "#D32F2F", BgColor = "#FFEBEE", Emoji = "🏥", Hotline = "137" }
                };

                await context.Operators.AddRangeAsync(operators);
                await context.SaveChangesAsync();

                // Retrieve Operators with IDs
                var opMap = operators.ToDictionary(o => o.Key, o => o.Id);

                // 2. Seed Operator Services
                var services = new List<OperatorService>();

                // Telecom Services
                foreach (var opKey in new[] { "vodafone", "orange", "etisalat", "we" })
                {
                    int opId = opMap[opKey];
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "new", Icon = "📲", NameAr = "استخراج خط جديد", EstimatedTime = "⏱ 20 دقيقة" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "sim", Icon = "🔄", NameAr = "استبدال شريحة SIM", EstimatedTime = "⏱ 10 دقائق" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "pkg", Icon = "📶", NameAr = "تجديد / ترقية باقة", EstimatedTime = "⏱ فوري" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "bill", Icon = "💸", NameAr = "دفع الفواتير", EstimatedTime = "⏱ فوري" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "trans", Icon = "🔁", NameAr = "نقل ملكية الخط", EstimatedTime = "⏱ 30 دقيقة" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "comp", Icon = "📢", NameAr = "تقديم شكوى", EstimatedTime = "⏱ رد 48 ساعة" });
                    services.Add(new OperatorService { OperatorId = opId, ServiceKey = "esim", Icon = "💡", NameAr = "تفعيل eSIM", EstimatedTime = "⏱ 15 دقيقة" });
                    if (opKey == "vodafone" || opKey == "orange")
                    {
                        services.Add(new OperatorService { OperatorId = opId, ServiceKey = "roam", Icon = "🌍", NameAr = "تفعيل التجوال الدولي", EstimatedTime = "⏱ فوري" });
                    }
                    if (opKey == "etisalat" || opKey == "we")
                    {
                        services.Add(new OperatorService { OperatorId = opId, ServiceKey = "fiber", Icon = "🌐", NameAr = "اشتراك Fiber/ADSL", EstimatedTime = "⏱ يوم عمل" });
                    }
                    if (opKey == "we")
                    {
                        services.Add(new OperatorService { OperatorId = opId, ServiceKey = "land", Icon = "☎️", NameAr = "خط أرضي جديد", EstimatedTime = "⏱ 3 أيام" });
                    }
                }

                // Civil Registry Services
                int civilOpId = opMap["civil_registry"];
                services.Add(new OperatorService { OperatorId = civilOpId, ServiceKey = "id_card", NameAr = "إصدار بطاقة الرقم القومي", Icon = "🪪", EstimatedTime = "⏱ 30 دقيقة" });
                services.Add(new OperatorService { OperatorId = civilOpId, ServiceKey = "birth_cert", NameAr = "إصدار شهادة الميلاد المميكنة", Icon = "👶", EstimatedTime = "⏱ 20 دقيقة" });
                services.Add(new OperatorService { OperatorId = civilOpId, ServiceKey = "marriage", NameAr = "وثيقة الزواج", Icon = "💍", EstimatedTime = "⏱ 45 دقيقة" });
                services.Add(new OperatorService { OperatorId = civilOpId, ServiceKey = "divorce", NameAr = "وثيقة الطلاق", Icon = "📜", EstimatedTime = "⏱ 20 دقيقة" });
                services.Add(new OperatorService { OperatorId = civilOpId, ServiceKey = "family_record", NameAr = "قيد عائلي", Icon = "👨‍👩‍👧‍👦", EstimatedTime = "⏱ 15 دقيقة" });

                // Bank Services (for NBE, Banque Misr, CIB, QNB, Alex Bank)
                foreach (var bankKey in new[] { "nbe", "banquemisr", "cib", "qnb", "alexbank" })
                {
                    int bankOpId = opMap[bankKey];
                    services.Add(new OperatorService { OperatorId = bankOpId, ServiceKey = "open_acc", NameAr = "فتح حساب جديد", Icon = "🏦", EstimatedTime = "⏱ 30 دقيقة" });
                    services.Add(new OperatorService { OperatorId = bankOpId, ServiceKey = "card_issue", NameAr = "إصدار بطاقات الائتمان", Icon = "💳", EstimatedTime = "⏱ 15 دقيقة" });
                    services.Add(new OperatorService { OperatorId = bankOpId, ServiceKey = "loans", NameAr = "القروض والتمويل", Icon = "💰", EstimatedTime = "⏱ 45 دقيقة" });
                    services.Add(new OperatorService { OperatorId = bankOpId, ServiceKey = "teller", NameAr = "خدمات الصراف (سحب/إيداع)", Icon = "💵", EstimatedTime = "⏱ 10 دقائق" });
                    services.Add(new OperatorService { OperatorId = bankOpId, ServiceKey = "customer_svc", NameAr = "خدمة العملاء", Icon = "👥", EstimatedTime = "⏱ 20 دقيقة" });
                }

                // Hospital Services
                int hospOpId = opMap["hospital_generic"];
                services.Add(new OperatorService { OperatorId = hospOpId, ServiceKey = "er", NameAr = "الطوارئ", Icon = "🚑", EstimatedTime = "⏱ 15 دقيقة" });
                services.Add(new OperatorService { OperatorId = hospOpId, ServiceKey = "outpatient", NameAr = "العيادات الخارجية", Icon = "🩺", EstimatedTime = "⏱ 40 دقيقة" });
                services.Add(new OperatorService { OperatorId = hospOpId, ServiceKey = "lab", NameAr = "التحاليل الطبية", Icon = "🧪", EstimatedTime = "⏱ 20 دقيقة" });
                services.Add(new OperatorService { OperatorId = hospOpId, ServiceKey = "xray", NameAr = "الأشعة والتشخيص", Icon = "🩻", EstimatedTime = "⏱ 30 دقيقة" });
                services.Add(new OperatorService { OperatorId = hospOpId, ServiceKey = "pharmacy", NameAr = "الصيدلية", Icon = "💊", EstimatedTime = "⏱ 10 دقائق" });

                await context.OperatorServices.AddRangeAsync(services);

                // 3. Seed Service Documents
                var docs = new List<ServiceDocument>
                {
                    new ServiceDocument { ServiceKey = "new", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي سارية المفعول", NoteAr = "أصل + صورة", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "new", DocType = DocType.Required, TextAr = "الحضور الشخصي إلزامي", NoteAr = "لا يمكن التوكيل", SortOrder = 2 },
                    new ServiceDocument { ServiceKey = "new", DocType = DocType.Optional, TextAr = "صورة إضافية للبطاقة", NoteAr = "للحفظ في السجلات", SortOrder = 3 },
                    new ServiceDocument { ServiceKey = "sim", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي", NoteAr = "أصل + صورة", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "sim", DocType = DocType.Required, TextAr = "حضور صاحب الخط شخصيا", NoteAr = "لا تقبل التوكيلات", SortOrder = 2 },
                    new ServiceDocument { ServiceKey = "pkg", DocType = DocType.Required, TextAr = "رقم الخط المراد تجديده", NoteAr = "يجب أن يكون باسمك", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "bill", DocType = DocType.Required, TextAr = "رقم الخط أو رقم الحساب", NoteAr = "موجود في الفاتورة", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "trans", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي للمالك الجديد", NoteAr = "أصل + صورتان", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "trans", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي للمالك الحالي", NoteAr = "أصل + صورة", SortOrder = 2 },
                    // Civil Registry Docs
                    new ServiceDocument { ServiceKey = "id_card", DocType = DocType.Required, TextAr = "استمارة الرقم القومي معتمدة", NoteAr = "تباع في السجل", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "id_card", DocType = DocType.Required, TextAr = "مستند إثبات شخصية أو حضور ضامن", NoteAr = "من الأقارب حتى الدرجة الرابعة", SortOrder = 2 },
                    new ServiceDocument { ServiceKey = "birth_cert", DocType = DocType.Required, TextAr = "طلب إصدار شهادة الميلاد", NoteAr = "يسحب مجاناً", SortOrder = 1 },
                    new ServiceDocument { ServiceKey = "birth_cert", DocType = DocType.Required, TextAr = "صورة بطاقة الرقم القومي للمستخرج", NoteAr = "أو الأب أو الأم", SortOrder = 2 }
                };

                await context.ServiceDocuments.AddRangeAsync(docs);

                // 4. Seed Districts
                var districts = new List<District>
                {
                    // Cairo (1)
                    new District { GovernorateId = cairo.Id, NameAr = "مدينة نصر", Type = "حي" },
                    new District { GovernorateId = cairo.Id, NameAr = "المعادي", Type = "حي" },
                    new District { GovernorateId = cairo.Id, NameAr = "مصر الجديدة", Type = "حي" },
                    new District { GovernorateId = cairo.Id, NameAr = "وسط البلد", Type = "قسم" },
                    new District { GovernorateId = cairo.Id, NameAr = "الزمالك", Type = "حي" },
                    new District { GovernorateId = cairo.Id, NameAr = "التجمع الخامس", Type = "مدينة" },
                    new District { GovernorateId = cairo.Id, NameAr = "شبرا", Type = "حي" },
                    // Giza (2)
                    new District { GovernorateId = giza.Id, NameAr = "الدقي والمهندسين", Type = "حي" },
                    new District { GovernorateId = giza.Id, NameAr = "الهرم", Type = "حي" },
                    new District { GovernorateId = giza.Id, NameAr = "6 أكتوبر", Type = "مدينة" },
                    new District { GovernorateId = giza.Id, NameAr = "الشيخ زايد", Type = "مدينة" },
                    // Alexandria (3)
                    new District { GovernorateId = alex.Id, NameAr = "سيدي جابر", Type = "حي" },
                    new District { GovernorateId = alex.Id, NameAr = "المنتزه", Type = "حي" },
                    new District { GovernorateId = alex.Id, NameAr = "وسط الإسكندرية", Type = "حي" },
                    // Qalyubia (8)
                    new District { GovernorateId = qalyubia.Id, NameAr = "بنها", Type = "مدينة" },
                    new District { GovernorateId = qalyubia.Id, NameAr = "العبور", Type = "مدينة" },
                    // Gharbia (9)
                    new District { GovernorateId = gharbia.Id, NameAr = "طنطا", Type = "مدينة" }
                };

                await context.Districts.AddRangeAsync(districts);
                await context.SaveChangesAsync();

                // Retrieve Districts map
                var distMap = districts.GroupBy(d => d.NameAr).ToDictionary(g => g.Key, g => g.First().Id);

                // Helper to get district ID
                int GetDistId(string name) => distMap.TryGetValue(name, out var id) ? id : districts[0].Id;

                // 5. Seed Branches
                var branches = new List<Branch>();

                // Vodafone Branches
                int vId = opMap["vodafone"];
                branches.Add(new Branch { Name = "فودافون - فرع مدينة نصر", Address = "شارع عباس العقاد، مدينة نصر", NameAr = "فرع عباس العقاد", Area = "مدينة نصر", Latitude = 30.0566, Longitude = 31.3411, GovernorateId = cairo.Id, DistrictId = GetDistId("مدينة نصر"), OperatorId = vId, Status = BranchStatus.Open, WaitTime = "25 دقيقة", QueueCount = 8, DistanceKm = 1.2, Rating = 4.2 });
                branches.Add(new Branch { Name = "فودافون - فرع المهندسين", Address = "شارع جامعة الدول العربية، المهندسين", NameAr = "فرع جامعة الدول", Area = "المهندسين", Latitude = 30.0522, Longitude = 31.2012, GovernorateId = giza.Id, DistrictId = GetDistId("الدقي والمهندسين"), OperatorId = vId, Status = BranchStatus.Open, WaitTime = "15 دقيقة", QueueCount = 4, DistanceKm = 2.5, Rating = 4.5 });
                branches.Add(new Branch { Name = "فودافون - فرع المعادي", Address = "شارع 9، المعادي", NameAr = "فرع شارع 9", Area = "المعادي", Latitude = 29.9602, Longitude = 31.2611, GovernorateId = cairo.Id, DistrictId = GetDistId("المعادي"), OperatorId = vId, Status = BranchStatus.Open, WaitTime = "5 دقائق", QueueCount = 1, DistanceKm = 4.0, Rating = 3.9 });

                // Orange Branches
                int oId = opMap["orange"];
                branches.Add(new Branch { Name = "أورنج - فرع مصر الجديدة", Address = "شارع الأهرام، الكوربة، مصر الجديدة", NameAr = "فرع الكوربة", Area = "مصر الجديدة", Latitude = 30.0901, Longitude = 31.3255, GovernorateId = cairo.Id, DistrictId = GetDistId("مصر الجديدة"), OperatorId = oId, Status = BranchStatus.Open, WaitTime = "10 دقائق", QueueCount = 3, DistanceKm = 3.1, Rating = 4.1 });
                branches.Add(new Branch { Name = "أورنج - فرع الدقي", Address = "شارع التحرير، الدقي", NameAr = "فرع التحرير", Area = "الدقي", Latitude = 30.0388, Longitude = 31.2111, GovernorateId = giza.Id, DistrictId = GetDistId("الدقي والمهندسين"), OperatorId = oId, Status = BranchStatus.Open, WaitTime = "30 دقيقة", QueueCount = 12, DistanceKm = 0.5, Rating = 3.8 });

                // Etisalat Branches
                int eId = opMap["etisalat"];
                branches.Add(new Branch { Name = "اتصالات - فرع التجمع الخامس", Address = "شارع التسعين الشمالي، التجمع الخامس", NameAr = "فرع التسعين", Area = "التجمع الخامس", Latitude = 30.0266, Longitude = 31.4811, GovernorateId = cairo.Id, DistrictId = GetDistId("التجمع الخامس"), OperatorId = eId, Status = BranchStatus.Open, WaitTime = "20 دقيقة", QueueCount = 6, DistanceKm = 5.2, Rating = 4.3 });

                // WE Branches
                int weId = opMap["we"];
                branches.Add(new Branch { Name = "وي - فرع وسط البلد", Address = "شارع طلعت حرب، وسط البلد", NameAr = "فرع طلعت حرب", Area = "وسط البلد", Latitude = 30.0466, Longitude = 31.2388, GovernorateId = cairo.Id, DistrictId = GetDistId("وسط البلد"), OperatorId = weId, Status = BranchStatus.Open, WaitTime = "12 دقيقة", QueueCount = 4, DistanceKm = 1.0, Rating = 4.0 });

                // Civil Registry (السجل المدني) Branches
                branches.Add(new Branch { Name = "سجل مدني مدينة نصر أول", Address = "بجوار قسم أول مدينة نصر، القاهرة", NameAr = "سجل مدني مدينة نصر", Area = "مدينة نصر", Latitude = 30.0592, Longitude = 31.3391, GovernorateId = cairo.Id, DistrictId = GetDistId("مدينة نصر"), OperatorId = civilOpId, Status = BranchStatus.Open, WaitTime = "45 دقيقة", QueueCount = 20, DistanceKm = 1.4, Rating = 3.5 });
                branches.Add(new Branch { Name = "سجل مدني مصر الجديدة", Address = "شارع الحجاز، مصر الجديدة", NameAr = "سجل مدني مصر الجديدة", Area = "مصر الجديدة", Latitude = 30.1002, Longitude = 31.3355, GovernorateId = cairo.Id, DistrictId = GetDistId("مصر الجديدة"), OperatorId = civilOpId, Status = BranchStatus.Open, WaitTime = "35 دقيقة", QueueCount = 15, DistanceKm = 2.8, Rating = 3.7 });
                branches.Add(new Branch { Name = "سجل مدني الدقي", Address = "بجوار مجلس مدينة الجيزة، الدقي", NameAr = "سجل مدني الدقي", Area = "الدقي", Latitude = 30.0411, Longitude = 31.2122, GovernorateId = giza.Id, DistrictId = GetDistId("الدقي والمهندسين"), OperatorId = civilOpId, Status = BranchStatus.Open, WaitTime = "50 دقيقة", QueueCount = 25, DistanceKm = 0.8, Rating = 3.2 });

                // Bank Branches (NBE, Banque Misr, CIB)
                int nbeId = opMap["nbe"];
                branches.Add(new Branch { Name = "البنك الأهلي المصري - فرع عباس العقاد", Address = "شارع عباس العقاد، مدينة نصر", NameAr = "فرع عباس العقاد", Area = "مدينة نصر", Latitude = 30.0571, Longitude = 31.3418, GovernorateId = cairo.Id, DistrictId = GetDistId("مدينة نصر"), OperatorId = nbeId, Status = BranchStatus.Open, WaitTime = "30 دقيقة", QueueCount = 10, DistanceKm = 1.3, Rating = 4.4 });

                int bmId = opMap["banquemisr"];
                branches.Add(new Branch { Name = "بنك مصر - فرع المهندسين", Address = "شارع البطل أحمد عبد العزيز، المهندسين", NameAr = "فرع البطل أحمد عبد العزيز", Area = "المهندسين", Latitude = 30.0531, Longitude = 31.2025, GovernorateId = giza.Id, DistrictId = GetDistId("الدقي والمهندسين"), OperatorId = bmId, Status = BranchStatus.Open, WaitTime = "25 دقيقة", QueueCount = 9, DistanceKm = 2.6, Rating = 4.1 });

                int cibId = opMap["cib"];
                branches.Add(new Branch { Name = "البنك التجاري الدولي CIB - فرع الزمالك", Address = "شارع 26 يوليو، الزمالك", NameAr = "فرع الزمالك", Area = "المالك", Latitude = 30.0601, Longitude = 31.2199, GovernorateId = cairo.Id, DistrictId = GetDistId("الزمالك"), OperatorId = cibId, Status = BranchStatus.Open, WaitTime = "15 دقيقة", QueueCount = 5, DistanceKm = 1.9, Rating = 4.6 });

                // Hospital Branches
                branches.Add(new Branch { Name = "مستشفى السلام الدولي - المعادي", Address = "كورنيش النيل، المعادي، القاهرة", NameAr = "مستشفى السلام الدولي", Area = "المعادي", Latitude = 29.9655, Longitude = 31.2501, GovernorateId = cairo.Id, DistrictId = GetDistId("المعادي"), OperatorId = hospOpId, Status = BranchStatus.Open, WaitTime = "10 دقائق", QueueCount = 2, DistanceKm = 3.5, Rating = 4.7 });

                await context.Branches.AddRangeAsync(branches);
                await context.SaveChangesAsync();

                // 6. Seed Branch Schedules
                var schedules = new List<BranchSchedule>();
                foreach (var b in branches)
                {
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Sunday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0), MaxAppointmentsPerSlot = 5, PeakStartTime = new TimeSpan(12, 0, 0), PeakEndTime = new TimeSpan(15, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Monday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0), MaxAppointmentsPerSlot = 5, PeakStartTime = new TimeSpan(12, 0, 0), PeakEndTime = new TimeSpan(15, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Tuesday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0), MaxAppointmentsPerSlot = 5, PeakStartTime = new TimeSpan(12, 0, 0), PeakEndTime = new TimeSpan(15, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Wednesday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0), MaxAppointmentsPerSlot = 5, PeakStartTime = new TimeSpan(12, 0, 0), PeakEndTime = new TimeSpan(15, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Thursday, OpenTime = new TimeSpan(9, 0, 0), CloseTime = new TimeSpan(17, 0, 0), MaxAppointmentsPerSlot = 5, PeakStartTime = new TimeSpan(12, 0, 0), PeakEndTime = new TimeSpan(15, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Friday, OpenTime = new TimeSpan(0, 0, 0), CloseTime = new TimeSpan(0, 0, 0), MaxAppointmentsPerSlot = 0, PeakStartTime = new TimeSpan(0, 0, 0), PeakEndTime = new TimeSpan(0, 0, 0) });
                    schedules.Add(new BranchSchedule { BranchId = b.Id, DayOfWeek = DayOfWeek.Saturday, OpenTime = new TimeSpan(0, 0, 0), CloseTime = new TimeSpan(0, 0, 0), MaxAppointmentsPerSlot = 0, PeakStartTime = new TimeSpan(0, 0, 0), PeakEndTime = new TimeSpan(0, 0, 0) });
                }
                await context.BranchSchedules.AddRangeAsync(schedules);
                await context.SaveChangesAsync();
            }

            // 7. Seed Courts
            if (!await context.Courts.AnyAsync())
            {
                var courts = new List<Court>
                {
                    new() { Name = "محكمة القاهرة الابتدائية", Type = CourtType.Civil, Address = "باب الخلق، القاهرة", Phone = "02-25940255", TotalRooms = 12, Latitude = 30.0466, Longitude = 31.2388 },
                    new() { Name = "محكمة الأسرة — العباسية", Type = CourtType.Family, Address = "العباسية، القاهرة", Phone = "02-24829100", TotalRooms = 6, Latitude = 30.0633, Longitude = 31.2755 },
                    new() { Name = "محكمة الجيزة الابتدائية", Type = CourtType.Civil, Address = "الجيزة — ميدان الجيزة", Phone = "02-35729800", TotalRooms = 10, Latitude = 30.0130, Longitude = 31.2082 },
                    new() { Name = "محكمة استئناف القاهرة", Type = CourtType.Appeal, Address = "باب الخلق، القاهرة", Phone = "02-25910022", TotalRooms = 8, Latitude = 30.0466, Longitude = 31.2388 },
                    new() { Name = "المحكمة الاقتصادية — القاهرة", Type = CourtType.Commercial, Address = "مدينة نصر، القاهرة", Phone = "02-24010999", TotalRooms = 4, Latitude = 30.0592, Longitude = 31.3391 },
                    new() { Name = "محكمة الجنايات — القاهرة", Type = CourtType.Criminal, Address = "باب الخلق، القاهرة", Phone = "02-25940255", TotalRooms = 7, Latitude = 30.0466, Longitude = 31.2388 },
                    new() { Name = "محكمة شمال القاهرة الابتدائية", Type = CourtType.Civil, Address = "مصر الجديدة، القاهرة", Phone = "02-24184900", TotalRooms = 9, Latitude = 30.1002, Longitude = 31.3355 },
                    new() { Name = "محكمة الأسرة — مدينة نصر", Type = CourtType.Family, Address = "مدينة نصر، القاهرة", Phone = "02-24011800", TotalRooms = 5, Latitude = 30.0592, Longitude = 31.3391 }
                };

                await context.Courts.AddRangeAsync(courts);
                await context.SaveChangesAsync();

                // Seed Court Departments
                var dept1 = new CourtDepartment { CourtId = courts[0].Id, Name = "دائرة مدنية أولى" };
                var dept2 = new CourtDepartment { CourtId = courts[0].Id, Name = "دائرة مدنية ثانية" };
                var dept3 = new CourtDepartment { CourtId = courts[1].Id, Name = "دائرة الأحوال الشخصية" };
                var dept4 = new CourtDepartment { CourtId = courts[2].Id, Name = "دائرة مدنية أولى" };
                var dept5 = new CourtDepartment { CourtId = courts[3].Id, Name = "دائرة الاستئناف العالي" };
                var dept6 = new CourtDepartment { CourtId = courts[4].Id, Name = "دائرة تجارية" };
                var dept7 = new CourtDepartment { CourtId = courts[5].Id, Name = "دائرة جنائية" };
                
                var departments = new List<CourtDepartment> { dept1, dept2, dept3, dept4, dept5, dept6, dept7 };
                await context.CourtDepartments.AddRangeAsync(departments);
                await context.SaveChangesAsync();

                // Seed Court Services
                var svc1 = new CourtService { Name = "جلسة قضائية", Description = "حضور جلسة المحاكمة أمام القاضي", EstimatedTime = TimeSpan.FromMinutes(60), RequiredDocumentsJson = "[\"بطاقة الرقم القومي سارية\",\"ملف القضية\",\"توكيل المحامي إن وجد\"]" };
                var svc2 = new CourtService { Name = "تقديم مستندات", Description = "تقديم أوراق ومستندات جديدة لملف القضية", EstimatedTime = TimeSpan.FromMinutes(30), RequiredDocumentsJson = "[\"بطاقة الرقم القومي\",\"أصول وصور المستندات المراد تقديمها\"]" };
                var svc3 = new CourtService { Name = "استخراج حكم", Description = "طلب الحصول على صورة رسمية من حكم المحكمة", EstimatedTime = TimeSpan.FromDays(3), RequiredDocumentsJson = "[\"بطاقة الرقم القومي\",\"رقم القضية وسنتها\",\"طلب رسمي مكتوب\"]" };
                var svc4 = new CourtService { Name = "توثيق مستند", Description = "توثيق العقود والوكالات الرسمية", EstimatedTime = TimeSpan.FromMinutes(45), RequiredDocumentsJson = "[\"بطاقة الرقم القومي للطرفين\",\"أصل المستند\",\"شاهدان\"]" };
                var svc5 = new CourtService { Name = "تقديم استئناف", Description = "الطعن في الحكم الابتدائي", EstimatedTime = TimeSpan.FromMinutes(45), RequiredDocumentsJson = "[\"نسخة الحكم المراد الطعن عليه\",\"مذكرة أسباب الطعن\"]" };
                var svc6 = new CourtService { Name = "وساطة وتحكيم", Description = "جلسة تسوية ودية للنزاعات", EstimatedTime = TimeSpan.FromMinutes(90), RequiredDocumentsJson = "[\"موافقة الطرفين على التحكيم\",\"ملخص النزاع\"]" };
                var svc7 = new CourtService { Name = "استشارة قانونية", Description = "جلسة استشارية مع المستشار القانوني للمحكمة", EstimatedTime = TimeSpan.FromMinutes(30), RequiredDocumentsJson = "[\"المستندات المتعلقة بالاستشارة\"]" };

                var servicesList = new List<CourtService> { svc1, svc2, svc3, svc4, svc5, svc6, svc7 };
                await context.CourtServices.AddRangeAsync(servicesList);
                await context.SaveChangesAsync();

                // Seed Legal Cases
                var cases = new List<LegalCase>
                {
                    new()
                    {
                        CaseNumber = "2025/4821",
                        Year = 2025,
                        CourtId = courts[0].Id,
                        DepartmentId = dept1.Id,
                        Type = "دعوى مدنية — محكمة القاهرة الابتدائية — دائرة مدنية أولى",
                        Status = "نشط",
                        Plaintiff = "أحمد محمد علي",
                        Defendant = "شركة النيل للتجارة",
                        TimelineEvents = new List<CaseTimelineEvent>
                        {
                            new() { Status = TimelineEventStatus.Done, Title = "إيداع الدعوى", EventDate = new DateTime(2025, 10, 10), Note = "" },
                            new() { Status = TimelineEventStatus.Done, Title = "قيد القضية بالجدول", EventDate = new DateTime(2025, 10, 12), Note = "" },
                            new() { Status = TimelineEventStatus.Done, Title = "الجلسة الأولى — تأجيل للإعلان", EventDate = new DateTime(2025, 11, 5), Note = "" },
                            new() { Status = TimelineEventStatus.Done, Title = "الجلسة الثانية — تقديم مستندات", EventDate = new DateTime(2025, 12, 10), Note = "" },
                            new() { Status = TimelineEventStatus.Active, Title = "الجلسة القادمة — مرافعة", EventDate = new DateTime(2026, 1, 20), Note = "الجلسة القادمة بعد 5 أيام — احضر قبل الموعد بـ 15 دقيقة" },
                            new() { Status = TimelineEventStatus.Pending, Title = "صدور الحكم المتوقع", EventDate = new DateTime(2026, 1, 20).AddDays(12), Note = "" }
                        }
                    },
                    new()
                    {
                        CaseNumber = "2024/1190",
                        Year = 2024,
                        CourtId = courts[1].Id,
                        DepartmentId = dept3.Id,
                        Type = "دعوى أسرة — محكمة الأسرة بالعباسية",
                        Status = "حكم صدر",
                        Plaintiff = "سارة أحمد حسن",
                        Defendant = "محمود علي محمود",
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

                await context.LegalCases.AddRangeAsync(cases);
                await context.SaveChangesAsync();
            }
        }
    }
}
