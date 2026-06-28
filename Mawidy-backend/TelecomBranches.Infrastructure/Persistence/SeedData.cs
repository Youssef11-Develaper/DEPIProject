using Microsoft.EntityFrameworkCore;
using TelecomBranches.Domain.Entities;
using TelecomBranches.Domain.Enums;

namespace TelecomBranches.Infrastructure.Persistence;

public static class SeedData
{
    public static void SeedAll(ModelBuilder mb)
    {
        SeedOperators(mb);
        SeedOperatorServices(mb);
        SeedServiceDocuments(mb);
        SeedGovernorates(mb);
        SeedDistricts(mb);
        SeedBranches(mb);
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static void SeedOperators(ModelBuilder mb)
    {
        mb.Entity<Operator>().HasData(
            new Operator { Id = 1, Key = "vodafone", NameAr = "فودافون",     Color = "#E53935", BgColor = "#ffebee", Emoji = "🔴", Hotline = "19888" },
            new Operator { Id = 2, Key = "orange",   NameAr = "أورنج",       Color = "#E65100", BgColor = "#fff3e0", Emoji = "🟠", Hotline = "16500" },
            new Operator { Id = 3, Key = "etisalat", NameAr = "اتصالات مصر", Color = "#1565C0", BgColor = "#e3f2fd", Emoji = "🔵", Hotline = "19500" },
            new Operator { Id = 4, Key = "we",       NameAr = "WE",          Color = "#2E7D32", BgColor = "#e8f5e9", Emoji = "🟢", Hotline = "15500" }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static void SeedOperatorServices(ModelBuilder mb)
    {
        mb.Entity<OperatorService>().HasData(
            // Vodafone
            new OperatorService { Id =  1, OperatorId = 1, ServiceKey = "new",   Icon = "📲", NameAr = "استخراج خط جديد",     EstimatedTime = "⏱ 20 دقيقة"   },
            new OperatorService { Id =  2, OperatorId = 1, ServiceKey = "sim",   Icon = "🔄", NameAr = "استبدال شريحة SIM",    EstimatedTime = "⏱ 10 دقائق"   },
            new OperatorService { Id =  3, OperatorId = 1, ServiceKey = "pkg",   Icon = "📶", NameAr = "تجديد / ترقية باقة",   EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id =  4, OperatorId = 1, ServiceKey = "bill",  Icon = "💸", NameAr = "دفع الفواتير",          EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id =  5, OperatorId = 1, ServiceKey = "trans", Icon = "🔁", NameAr = "نقل ملكية الخط",        EstimatedTime = "⏱ 30 دقيقة"   },
            new OperatorService { Id =  6, OperatorId = 1, ServiceKey = "comp",  Icon = "📢", NameAr = "تقديم شكوى",            EstimatedTime = "⏱ رد 48 ساعة" },
            new OperatorService { Id =  7, OperatorId = 1, ServiceKey = "esim",  Icon = "💡", NameAr = "تفعيل eSIM",            EstimatedTime = "⏱ 15 دقيقة"   },
            new OperatorService { Id =  8, OperatorId = 1, ServiceKey = "roam",  Icon = "🌍", NameAr = "تفعيل التجوال الدولي", EstimatedTime = "⏱ فوري"        },
            // Orange
            new OperatorService { Id =  9, OperatorId = 2, ServiceKey = "new",   Icon = "📲", NameAr = "استخراج خط جديد",     EstimatedTime = "⏱ 20 دقيقة"   },
            new OperatorService { Id = 10, OperatorId = 2, ServiceKey = "sim",   Icon = "🔄", NameAr = "استبدال شريحة SIM",    EstimatedTime = "⏱ 10 دقائق"   },
            new OperatorService { Id = 11, OperatorId = 2, ServiceKey = "pkg",   Icon = "📶", NameAr = "تجديد / ترقية باقة",   EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 12, OperatorId = 2, ServiceKey = "bill",  Icon = "💸", NameAr = "دفع الفواتير",          EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 13, OperatorId = 2, ServiceKey = "trans", Icon = "🔁", NameAr = "نقل ملكية الخط",        EstimatedTime = "⏱ 30 دقيقة"   },
            new OperatorService { Id = 14, OperatorId = 2, ServiceKey = "comp",  Icon = "📢", NameAr = "تقديم شكوى",            EstimatedTime = "⏱ رد 48 ساعة" },
            new OperatorService { Id = 15, OperatorId = 2, ServiceKey = "esim",  Icon = "💡", NameAr = "تفعيل eSIM",            EstimatedTime = "⏱ 15 دقيقة"   },
            // Etisalat
            new OperatorService { Id = 16, OperatorId = 3, ServiceKey = "new",   Icon = "📲", NameAr = "استخراج خط جديد",     EstimatedTime = "⏱ 20 دقيقة"   },
            new OperatorService { Id = 17, OperatorId = 3, ServiceKey = "sim",   Icon = "🔄", NameAr = "استبدال شريحة SIM",    EstimatedTime = "⏱ 10 دقائق"   },
            new OperatorService { Id = 18, OperatorId = 3, ServiceKey = "pkg",   Icon = "📶", NameAr = "تجديد / ترقية باقة",   EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 19, OperatorId = 3, ServiceKey = "bill",  Icon = "💸", NameAr = "دفع الفواتير",          EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 20, OperatorId = 3, ServiceKey = "trans", Icon = "🔁", NameAr = "نقل ملكية الخط",        EstimatedTime = "⏱ 30 دقيقة"   },
            new OperatorService { Id = 21, OperatorId = 3, ServiceKey = "comp",  Icon = "📢", NameAr = "تقديم شكوى",            EstimatedTime = "⏱ رد 48 ساعة" },
            new OperatorService { Id = 22, OperatorId = 3, ServiceKey = "fiber", Icon = "🌐", NameAr = "اشتراك Fiber/ADSL",    EstimatedTime = "⏱ يوم عمل"    },
            // WE
            new OperatorService { Id = 23, OperatorId = 4, ServiceKey = "new",   Icon = "📲", NameAr = "استخراج خط جديد",      EstimatedTime = "⏱ 20 دقيقة"   },
            new OperatorService { Id = 24, OperatorId = 4, ServiceKey = "sim",   Icon = "🔄", NameAr = "استبدال شريحة SIM",     EstimatedTime = "⏱ 10 دقائق"   },
            new OperatorService { Id = 25, OperatorId = 4, ServiceKey = "pkg",   Icon = "📶", NameAr = "تجديد / ترقية باقة",    EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 26, OperatorId = 4, ServiceKey = "bill",  Icon = "💸", NameAr = "دفع الفواتير",           EstimatedTime = "⏱ فوري"        },
            new OperatorService { Id = 27, OperatorId = 4, ServiceKey = "trans", Icon = "🔁", NameAr = "نقل ملكية الخط",         EstimatedTime = "⏱ 30 دقيقة"   },
            new OperatorService { Id = 28, OperatorId = 4, ServiceKey = "comp",  Icon = "📢", NameAr = "تقديم شكوى",             EstimatedTime = "⏱ رد 48 ساعة" },
            new OperatorService { Id = 29, OperatorId = 4, ServiceKey = "fiber", Icon = "🌐", NameAr = "إنترنت أرضي WE HOME",   EstimatedTime = "⏱ يومان"       },
            new OperatorService { Id = 30, OperatorId = 4, ServiceKey = "land",  Icon = "☎️", NameAr = "خط أرضي جديد",           EstimatedTime = "⏱ 3 أيام"     }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static void SeedServiceDocuments(ModelBuilder mb)
    {
        mb.Entity<ServiceDocument>().HasData(
            new ServiceDocument { Id =  1, ServiceKey = "new",   DocType = DocType.Required, TextAr = "بطاقة الرقم القومي سارية المفعول",      NoteAr = "أصل + صورة",                      SortOrder = 1 },
            new ServiceDocument { Id =  2, ServiceKey = "new",   DocType = DocType.Required, TextAr = "الحضور الشخصي إلزامي",                  NoteAr = "لا يمكن التوكيل",                 SortOrder = 2 },
            new ServiceDocument { Id =  3, ServiceKey = "new",   DocType = DocType.Optional, TextAr = "صورة إضافية للبطاقة",                    NoteAr = "للحفظ في السجلات",                SortOrder = 3 },
            new ServiceDocument { Id =  4, ServiceKey = "sim",   DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل + صورة",                      SortOrder = 1 },
            new ServiceDocument { Id =  5, ServiceKey = "sim",   DocType = DocType.Required, TextAr = "حضور صاحب الخط شخصيا",                  NoteAr = "لا تقبل التوكيلات",               SortOrder = 2 },
            new ServiceDocument { Id =  6, ServiceKey = "sim",   DocType = DocType.Required, TextAr = "رقم الخط المراد استبداله",               NoteAr = "يكفي إثبات الملكية",              SortOrder = 3 },
            new ServiceDocument { Id =  7, ServiceKey = "sim",   DocType = DocType.Optional, TextAr = "الشريحة القديمة التالفة",                NoteAr = "ليست إلزامية",                    SortOrder = 4 },
            new ServiceDocument { Id =  8, ServiceKey = "pkg",   DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل أو صورة واضحة",              SortOrder = 1 },
            new ServiceDocument { Id =  9, ServiceKey = "pkg",   DocType = DocType.Required, TextAr = "رقم الخط المراد تجديده",                 NoteAr = "يجب أن يكون باسمك",               SortOrder = 2 },
            new ServiceDocument { Id = 10, ServiceKey = "pkg",   DocType = DocType.Optional, TextAr = "الحضور الشخصي",                          NoteAr = "أو بتوكيل رسمي",                  SortOrder = 3 },
            new ServiceDocument { Id = 11, ServiceKey = "bill",  DocType = DocType.Required, TextAr = "رقم الخط أو رقم الحساب",                NoteAr = "موجود في الفاتورة",               SortOrder = 1 },
            new ServiceDocument { Id = 12, ServiceKey = "bill",  DocType = DocType.Required, TextAr = "قيمة الفاتورة",                          NoteAr = "نقدا أو بطاقة بنكية",             SortOrder = 2 },
            new ServiceDocument { Id = 13, ServiceKey = "bill",  DocType = DocType.Optional, TextAr = "بطاقة الرقم القومي",                     NoteAr = "ليست إلزامية للدفع",              SortOrder = 3 },
            new ServiceDocument { Id = 14, ServiceKey = "trans", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي للمالك الجديد",       NoteAr = "أصل + صورتان",                    SortOrder = 1 },
            new ServiceDocument { Id = 15, ServiceKey = "trans", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي للمالك الحالي",       NoteAr = "أصل + صورة",                      SortOrder = 2 },
            new ServiceDocument { Id = 16, ServiceKey = "trans", DocType = DocType.Required, TextAr = "حضور الطرفين أو توكيل رسمي",             NoteAr = "من الشهر العقاري",                SortOrder = 3 },
            new ServiceDocument { Id = 17, ServiceKey = "trans", DocType = DocType.Required, TextAr = "سداد أي مديونيات على الخط",              NoteAr = "لا يحول الخط وعليه متأخرات",     SortOrder = 4 },
            new ServiceDocument { Id = 18, ServiceKey = "comp",  DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل أو صورة",                     SortOrder = 1 },
            new ServiceDocument { Id = 19, ServiceKey = "comp",  DocType = DocType.Required, TextAr = "رقم الخط أو رقم الحساب",                NoteAr = null,                               SortOrder = 2 },
            new ServiceDocument { Id = 20, ServiceKey = "comp",  DocType = DocType.Optional, TextAr = "لقطات شاشة أو مستندات",                 NoteAr = "تسرع البت في الشكوى",            SortOrder = 3 },
            new ServiceDocument { Id = 21, ServiceKey = "esim",  DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل + صورة",                      SortOrder = 1 },
            new ServiceDocument { Id = 22, ServiceKey = "esim",  DocType = DocType.Required, TextAr = "جهاز يدعم eSIM",                         NoteAr = "iPhone XS+ أو أندرويد مدعوم",    SortOrder = 2 },
            new ServiceDocument { Id = 23, ServiceKey = "esim",  DocType = DocType.Required, TextAr = "الحضور الشخصي لمسح QR Code",             NoteAr = "لا يرسل الـ QR عن بعد",          SortOrder = 3 },
            new ServiceDocument { Id = 24, ServiceKey = "roam",  DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = null,                               SortOrder = 1 },
            new ServiceDocument { Id = 25, ServiceKey = "roam",  DocType = DocType.Required, TextAr = "رقم الخط المراد تفعيل التجوال عليه",    NoteAr = "يجب أن يكون باسمك",               SortOrder = 2 },
            new ServiceDocument { Id = 26, ServiceKey = "roam",  DocType = DocType.Optional, TextAr = "اسم الدول المراد السفر إليها",            NoteAr = "لاختيار أنسب الباقات",            SortOrder = 3 },
            new ServiceDocument { Id = 27, ServiceKey = "fiber", DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل + صورة",                      SortOrder = 1 },
            new ServiceDocument { Id = 28, ServiceKey = "fiber", DocType = DocType.Required, TextAr = "عقد الإيجار أو عقد الملكية",             NoteAr = "لإثبات عنوان التركيب",            SortOrder = 2 },
            new ServiceDocument { Id = 29, ServiceKey = "fiber", DocType = DocType.Required, TextAr = "فاتورة كهرباء أو مياه حديثة",            NoteAr = "خلال آخر 3 أشهر",                 SortOrder = 3 },
            new ServiceDocument { Id = 30, ServiceKey = "land",  DocType = DocType.Required, TextAr = "بطاقة الرقم القومي",                     NoteAr = "أصل + صورتان",                    SortOrder = 1 },
            new ServiceDocument { Id = 31, ServiceKey = "land",  DocType = DocType.Required, TextAr = "عقد الإيجار أو عقد الملكية",             NoteAr = null,                               SortOrder = 2 },
            new ServiceDocument { Id = 32, ServiceKey = "land",  DocType = DocType.Required, TextAr = "فاتورة كهرباء حديثة",                    NoteAr = "خلال آخر 3 أشهر",                 SortOrder = 3 },
            new ServiceDocument { Id = 33, ServiceKey = "land",  DocType = DocType.Required, TextAr = "سداد رسوم التركيب",                       NoteAr = "150-300 جنيه حسب المنطقة",        SortOrder = 4 }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static void SeedGovernorates(ModelBuilder mb)
    {
        mb.Entity<Governorate>().HasData(
            new Governorate { Id =  1, NameAr = "القاهرة",       NameEn = "Cairo",          Region = "القاهرة الكبرى",  Emoji = "🏛", SortOrder =  1 },
            new Governorate { Id =  2, NameAr = "الجيزة",        NameEn = "Giza",           Region = "القاهرة الكبرى",  Emoji = "🗿", SortOrder =  2 },
            new Governorate { Id =  3, NameAr = "القليوبية",     NameEn = "Qalyubia",       Region = "القاهرة الكبرى",  Emoji = "🌆", SortOrder =  3 },
            new Governorate { Id =  4, NameAr = "الإسكندرية",    NameEn = "Alexandria",     Region = "الساحل الشمالي",  Emoji = "🏖", SortOrder =  4 },
            new Governorate { Id =  5, NameAr = "البحيرة",       NameEn = "Beheira",        Region = "الدلتا",           Emoji = "🌾", SortOrder =  5 },
            new Governorate { Id =  6, NameAr = "الغربية",       NameEn = "Gharbia",        Region = "الدلتا",           Emoji = "🏙", SortOrder =  6 },
            new Governorate { Id =  7, NameAr = "المنوفية",      NameEn = "Monufia",        Region = "الدلتا",           Emoji = "🌿", SortOrder =  7 },
            new Governorate { Id =  8, NameAr = "الدقهلية",      NameEn = "Dakahlia",       Region = "الدلتا",           Emoji = "🌊", SortOrder =  8 },
            new Governorate { Id =  9, NameAr = "الشرقية",       NameEn = "Sharqia",        Region = "الدلتا",           Emoji = "🌻", SortOrder =  9 },
            new Governorate { Id = 10, NameAr = "كفر الشيخ",     NameEn = "Kafr el-Sheikh", Region = "الدلتا",           Emoji = "🐟", SortOrder = 10 },
            new Governorate { Id = 11, NameAr = "دمياط",         NameEn = "Damietta",       Region = "الدلتا",           Emoji = "⚓", SortOrder = 11 },
            new Governorate { Id = 12, NameAr = "الإسماعيلية",   NameEn = "Ismailia",       Region = "قناة السويس",      Emoji = "🚢", SortOrder = 12 },
            new Governorate { Id = 13, NameAr = "بورسعيد",       NameEn = "Port Said",      Region = "قناة السويس",      Emoji = "🔵", SortOrder = 13 },
            new Governorate { Id = 14, NameAr = "السويس",        NameEn = "Suez",           Region = "قناة السويس",      Emoji = "⛽", SortOrder = 14 },
            new Governorate { Id = 15, NameAr = "شمال سيناء",    NameEn = "North Sinai",    Region = "سيناء",            Emoji = "🏜", SortOrder = 15 },
            new Governorate { Id = 16, NameAr = "جنوب سيناء",    NameEn = "South Sinai",    Region = "سيناء",            Emoji = "🌴", SortOrder = 16 },
            new Governorate { Id = 17, NameAr = "الفيوم",        NameEn = "Faiyum",         Region = "الصعيد",           Emoji = "🌺", SortOrder = 17 },
            new Governorate { Id = 18, NameAr = "بني سويف",      NameEn = "Beni Suef",      Region = "الصعيد",           Emoji = "🏺", SortOrder = 18 },
            new Governorate { Id = 19, NameAr = "المنيا",        NameEn = "Minya",          Region = "الصعيد",           Emoji = "🏛", SortOrder = 19 },
            new Governorate { Id = 20, NameAr = "أسيوط",         NameEn = "Asyut",          Region = "الصعيد",           Emoji = "🦅", SortOrder = 20 },
            new Governorate { Id = 21, NameAr = "سوهاج",         NameEn = "Sohag",          Region = "الصعيد",           Emoji = "🌾", SortOrder = 21 },
            new Governorate { Id = 22, NameAr = "قنا",           NameEn = "Qena",           Region = "الصعيد",           Emoji = "🏺", SortOrder = 22 },
            new Governorate { Id = 23, NameAr = "الأقصر",        NameEn = "Luxor",          Region = "الصعيد",           Emoji = "🛕", SortOrder = 23 },
            new Governorate { Id = 24, NameAr = "أسوان",         NameEn = "Aswan",          Region = "الصعيد",           Emoji = "🌊", SortOrder = 24 },
            new Governorate { Id = 25, NameAr = "مطروح",         NameEn = "Matrouh",        Region = "الساحل الشمالي",  Emoji = "🏝", SortOrder = 25 },
            new Governorate { Id = 26, NameAr = "البحر الأحمر",  NameEn = "Red Sea",        Region = "الساحل الشرقي",   Emoji = "🐠", SortOrder = 26 },
            new Governorate { Id = 27, NameAr = "الوادي الجديد", NameEn = "New Valley",     Region = "الصحراء الغربية", Emoji = "🏜", SortOrder = 27 }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static void SeedDistricts(ModelBuilder mb)
    {
        mb.Entity<District>().HasData(
            // 1 القاهرة
            new District { Id =   1, GovernorateId =  1, NameAr = "مدينة نصر",                    Type = "حي"    },
            new District { Id =   2, GovernorateId =  1, NameAr = "المعادي",                       Type = "حي"    },
            new District { Id =   3, GovernorateId =  1, NameAr = "مصر الجديدة",                   Type = "حي"    },
            new District { Id =   4, GovernorateId =  1, NameAr = "وسط البلد",                     Type = "قسم"   },
            new District { Id =   5, GovernorateId =  1, NameAr = "شبرا",                          Type = "حي"    },
            new District { Id =   6, GovernorateId =  1, NameAr = "الزمالك",                       Type = "حي"    },
            new District { Id =   7, GovernorateId =  1, NameAr = "المنيل",                        Type = "حي"    },
            new District { Id =   8, GovernorateId =  1, NameAr = "التجمع الخامس",                 Type = "مدينة" },
            new District { Id =   9, GovernorateId =  1, NameAr = "المقطم",                        Type = "حي"    },
            new District { Id =  10, GovernorateId =  1, NameAr = "عين شمس",                       Type = "حي"    },
            // 2 الجيزة
            new District { Id =  11, GovernorateId =  2, NameAr = "الدقي والمهندسين",              Type = "حي"    },
            new District { Id =  12, GovernorateId =  2, NameAr = "الهرم",                         Type = "حي"    },
            new District { Id =  13, GovernorateId =  2, NameAr = "فيصل",                          Type = "حي"    },
            new District { Id =  14, GovernorateId =  2, NameAr = "6 أكتوبر",                      Type = "مدينة" },
            new District { Id =  15, GovernorateId =  2, NameAr = "الشيخ زايد",                    Type = "مدينة" },
            new District { Id =  16, GovernorateId =  2, NameAr = "الوراق",                        Type = "مركز"  },
            // 3 القليوبية
            new District { Id =  17, GovernorateId =  3, NameAr = "بنها",                          Type = "مدينة" },
            new District { Id =  18, GovernorateId =  3, NameAr = "شبرا الخيمة",                   Type = "مدينة" },
            new District { Id =  19, GovernorateId =  3, NameAr = "القناطر الخيرية",               Type = "مركز"  },
            new District { Id =  20, GovernorateId =  3, NameAr = "طوخ",                           Type = "مركز"  },
            new District { Id =  21, GovernorateId =  3, NameAr = "قليوب",                         Type = "مركز"  },
            new District { Id =  22, GovernorateId =  3, NameAr = "العبور",                        Type = "مدينة" },
            // 4 الإسكندرية
            new District { Id =  23, GovernorateId =  4, NameAr = "المنتزه",                       Type = "حي"    },
            new District { Id =  24, GovernorateId =  4, NameAr = "سيدي جابر",                     Type = "حي"    },
            new District { Id =  25, GovernorateId =  4, NameAr = "وسط الإسكندرية",                Type = "حي"    },
            new District { Id =  26, GovernorateId =  4, NameAr = "العجمي",                        Type = "حي"    },
            new District { Id =  27, GovernorateId =  4, NameAr = "برج العرب",                     Type = "مدينة" },
            new District { Id =  28, GovernorateId =  4, NameAr = "ستانلي",                        Type = "حي"    },
            // 5 البحيرة
            new District { Id =  29, GovernorateId =  5, NameAr = "دمنهور",                        Type = "مدينة" },
            new District { Id =  30, GovernorateId =  5, NameAr = "كفر الدوار",                    Type = "مركز"  },
            new District { Id =  31, GovernorateId =  5, NameAr = "إيتاي البارود",                 Type = "مركز"  },
            new District { Id =  32, GovernorateId =  5, NameAr = "رشيد",                          Type = "مركز"  },
            // 6 الغربية
            new District { Id =  33, GovernorateId =  6, NameAr = "طنطا",                          Type = "مدينة" },
            new District { Id =  34, GovernorateId =  6, NameAr = "المحلة الكبرى",                 Type = "مدينة" },
            new District { Id =  35, GovernorateId =  6, NameAr = "كفر الزيات",                    Type = "مركز"  },
            new District { Id =  36, GovernorateId =  6, NameAr = "سمنود",                         Type = "مركز"  },
            // 7 المنوفية
            new District { Id =  37, GovernorateId =  7, NameAr = "شبين الكوم",                    Type = "مدينة" },
            new District { Id =  38, GovernorateId =  7, NameAr = "منوف",                          Type = "مركز"  },
            new District { Id =  39, GovernorateId =  7, NameAr = "أشمون",                         Type = "مركز"  },
            new District { Id =  40, GovernorateId =  7, NameAr = "قويسنا",                        Type = "مركز"  },
            // 8 الدقهلية
            new District { Id =  41, GovernorateId =  8, NameAr = "المنصورة",                      Type = "مدينة" },
            new District { Id =  42, GovernorateId =  8, NameAr = "طلخا",                          Type = "مدينة" },
            new District { Id =  43, GovernorateId =  8, NameAr = "ميت غمر",                       Type = "مركز"  },
            new District { Id =  44, GovernorateId =  8, NameAr = "المنزلة",                       Type = "مركز"  },
            // 9 الشرقية
            new District { Id =  45, GovernorateId =  9, NameAr = "الزقازيق",                      Type = "مدينة" },
            new District { Id =  46, GovernorateId =  9, NameAr = "العاشر من رمضان",               Type = "مدينة" },
            new District { Id =  47, GovernorateId =  9, NameAr = "بلبيس",                         Type = "مركز"  },
            new District { Id =  48, GovernorateId =  9, NameAr = "أبو كبير",                      Type = "مركز"  },
            // 10 كفر الشيخ
            new District { Id =  49, GovernorateId = 10, NameAr = "كفر الشيخ المدينة",             Type = "مدينة" },
            new District { Id =  50, GovernorateId = 10, NameAr = "دسوق",                          Type = "مركز"  },
            new District { Id =  51, GovernorateId = 10, NameAr = "فوه",                           Type = "مركز"  },
            // 11 دمياط
            new District { Id =  52, GovernorateId = 11, NameAr = "دمياط المدينة",                 Type = "مدينة" },
            new District { Id =  53, GovernorateId = 11, NameAr = "رأس البر",                      Type = "مدينة" },
            new District { Id =  54, GovernorateId = 11, NameAr = "فارسكور",                       Type = "مركز"  },
            // 12 الإسماعيلية
            new District { Id =  55, GovernorateId = 12, NameAr = "الإسماعيلية المدينة",           Type = "مدينة" },
            new District { Id =  56, GovernorateId = 12, NameAr = "القنطرة شرق",                   Type = "مركز"  },
            new District { Id =  57, GovernorateId = 12, NameAr = "فايد",                          Type = "مركز"  },
            // 13 بورسعيد
            new District { Id =  58, GovernorateId = 13, NameAr = "بورسعيد حي الشرق",             Type = "حي"    },
            new District { Id =  59, GovernorateId = 13, NameAr = "بورسعيد حي الغرب",             Type = "حي"    },
            new District { Id =  60, GovernorateId = 13, NameAr = "بور فؤاد",                      Type = "حي"    },
            // 14 السويس
            new District { Id =  61, GovernorateId = 14, NameAr = "حي الأربعين",                   Type = "حي"    },
            new District { Id =  62, GovernorateId = 14, NameAr = "حي عتاقة",                      Type = "حي"    },
            new District { Id =  63, GovernorateId = 14, NameAr = "حي فيصل السويس",                Type = "حي"    },
            // 15 شمال سيناء
            new District { Id =  64, GovernorateId = 15, NameAr = "العريش",                        Type = "مدينة" },
            new District { Id =  65, GovernorateId = 15, NameAr = "الشيخ زويد",                    Type = "مركز"  },
            new District { Id =  66, GovernorateId = 15, NameAr = "رفح",                           Type = "مركز"  },
            // 16 جنوب سيناء
            new District { Id =  67, GovernorateId = 16, NameAr = "شرم الشيخ",                     Type = "مدينة" },
            new District { Id =  68, GovernorateId = 16, NameAr = "دهب",                           Type = "مدينة" },
            new District { Id =  69, GovernorateId = 16, NameAr = "طور سيناء",                     Type = "مدينة" },
            // 17 الفيوم
            new District { Id =  70, GovernorateId = 17, NameAr = "الفيوم المدينة",                Type = "مدينة" },
            new District { Id =  71, GovernorateId = 17, NameAr = "إبشواي",                        Type = "مركز"  },
            new District { Id =  72, GovernorateId = 17, NameAr = "سنورس",                         Type = "مركز"  },
            // 18 بني سويف
            new District { Id =  73, GovernorateId = 18, NameAr = "بني سويف المدينة",              Type = "مدينة" },
            new District { Id =  74, GovernorateId = 18, NameAr = "الفشن",                         Type = "مركز"  },
            new District { Id =  75, GovernorateId = 18, NameAr = "ناصر",                          Type = "مركز"  },
            // 19 المنيا
            new District { Id =  76, GovernorateId = 19, NameAr = "المنيا المدينة",                Type = "مدينة" },
            new District { Id =  77, GovernorateId = 19, NameAr = "أبو قرقاص",                     Type = "مركز"  },
            new District { Id =  78, GovernorateId = 19, NameAr = "ملوي",                          Type = "مركز"  },
            new District { Id =  79, GovernorateId = 19, NameAr = "سمالوط",                        Type = "مركز"  },
            new District { Id =  80, GovernorateId = 19, NameAr = "بني مزار",                      Type = "مركز"  },
            // 20 أسيوط
            new District { Id =  81, GovernorateId = 20, NameAr = "أسيوط المدينة",                 Type = "مدينة" },
            new District { Id =  82, GovernorateId = 20, NameAr = "ديروط",                         Type = "مركز"  },
            new District { Id =  83, GovernorateId = 20, NameAr = "منفلوط",                        Type = "مركز"  },
            new District { Id =  84, GovernorateId = 20, NameAr = "القوصية",                       Type = "مركز"  },
            // 21 سوهاج
            new District { Id =  85, GovernorateId = 21, NameAr = "سوهاج المدينة",                 Type = "مدينة" },
            new District { Id =  86, GovernorateId = 21, NameAr = "طهطا",                          Type = "مركز"  },
            new District { Id =  87, GovernorateId = 21, NameAr = "جرجا",                          Type = "مركز"  },
            new District { Id =  88, GovernorateId = 21, NameAr = "أخميم",                         Type = "مركز"  },
            // 22 قنا
            new District { Id =  89, GovernorateId = 22, NameAr = "قنا المدينة",                   Type = "مدينة" },
            new District { Id =  90, GovernorateId = 22, NameAr = "قوص",                           Type = "مركز"  },
            new District { Id =  91, GovernorateId = 22, NameAr = "نجع حمادي",                     Type = "مدينة" },
            // 23 الأقصر
            new District { Id =  92, GovernorateId = 23, NameAr = "الأقصر المدينة",                Type = "مدينة" },
            new District { Id =  93, GovernorateId = 23, NameAr = "إسنا",                          Type = "مركز"  },
            // 24 أسوان
            new District { Id =  94, GovernorateId = 24, NameAr = "أسوان المدينة",                 Type = "مدينة" },
            new District { Id =  95, GovernorateId = 24, NameAr = "كوم أمبو",                      Type = "مركز"  },
            new District { Id =  96, GovernorateId = 24, NameAr = "إدفو",                          Type = "مركز"  },
            // 25 مطروح
            new District { Id =  97, GovernorateId = 25, NameAr = "مرسى مطروح",                    Type = "مدينة" },
            new District { Id =  98, GovernorateId = 25, NameAr = "العلمين",                       Type = "مدينة" },
            new District { Id =  99, GovernorateId = 25, NameAr = "سيوة",                          Type = "مدينة" },
            // 26 البحر الأحمر
            new District { Id = 100, GovernorateId = 26, NameAr = "الغردقة",                       Type = "مدينة" },
            new District { Id = 101, GovernorateId = 26, NameAr = "سفاجا",                         Type = "مدينة" },
            new District { Id = 102, GovernorateId = 26, NameAr = "مرسى علم",                      Type = "مدينة" },
            // 27 الوادي الجديد
            new District { Id = 103, GovernorateId = 27, NameAr = "الخارجة",                       Type = "مدينة" },
            new District { Id = 104, GovernorateId = 27, NameAr = "الداخلة",                       Type = "مدينة" },
            new District { Id = 105, GovernorateId = 27, NameAr = "الفرافرة",                      Type = "مدينة" }
        );
    }

    // ─────────────────────────────────────────────────────────────────────────
    // الفروع — كل فرع مكتوب صريح بدون loops أو local functions
    private static void SeedBranches(ModelBuilder mb)
    {
        var list = new List<Branch>();
        int id = 1;

        // حالات دورية
        BranchStatus[] statuses = { BranchStatus.Open, BranchStatus.Open, BranchStatus.Busy, BranchStatus.Open, BranchStatus.Closed, BranchStatus.Open, BranchStatus.Busy, BranchStatus.Open };
        int[]    queues  = { 3, 7, 12, 5, 0, 9, 15, 2 };
        string[] waits   = { "5 د", "10 د", "18 د", "7 د", "—", "13 د", "22 د", "3 د" };
        double[] ratings = { 4.3, 4.1, 3.9, 4.5, 4.0, 4.2, 3.8, 4.6 };

        string[] opNames = { "", "فودافون", "أورنج", "اتصالات مصر", "WE" };

        // govId, distId, distName, govName, operators[]
        var rows = new (int G, int D, string DN, string GN, int[] Ops)[]
        {
            // القاهرة
            (1,  1,  "مدينة نصر",          "القاهرة",    new[]{1,2,3,4}),
            (1,  2,  "المعادي",             "القاهرة",    new[]{1,2,3,4}),
            (1,  3,  "مصر الجديدة",         "القاهرة",    new[]{1,2,3,4}),
            (1,  4,  "وسط البلد",           "القاهرة",    new[]{1,2,3,4}),
            (1,  5,  "شبرا",                "القاهرة",    new[]{1,2,3,4}),
            (1,  6,  "الزمالك",             "القاهرة",    new[]{1,2,3}  ),
            (1,  7,  "المنيل",              "القاهرة",    new[]{1,2,4}  ),
            (1,  8,  "التجمع الخامس",       "القاهرة",    new[]{1,2,3,4}),
            (1,  9,  "المقطم",              "القاهرة",    new[]{1,2,3}  ),
            (1, 10,  "عين شمس",             "القاهرة",    new[]{1,2,3,4}),
            // الجيزة
            (2, 11,  "الدقي والمهندسين",    "الجيزة",     new[]{1,2,3,4}),
            (2, 12,  "الهرم",               "الجيزة",     new[]{1,2,3,4}),
            (2, 13,  "فيصل",                "الجيزة",     new[]{1,2,3}  ),
            (2, 14,  "6 أكتوبر",            "الجيزة",     new[]{1,2,3,4}),
            (2, 15,  "الشيخ زايد",          "الجيزة",     new[]{1,2,4}  ),
            (2, 16,  "الوراق",              "الجيزة",     new[]{1,3}    ),
            // القليوبية
            (3, 17,  "بنها",                "القليوبية",  new[]{1,2,3,4}),
            (3, 18,  "شبرا الخيمة",         "القليوبية",  new[]{1,2,3,4}),
            (3, 19,  "القناطر الخيرية",     "القليوبية",  new[]{1,2}    ),
            (3, 20,  "طوخ",                 "القليوبية",  new[]{1,3}    ),
            (3, 21,  "قليوب",               "القليوبية",  new[]{2,4}    ),
            (3, 22,  "العبور",              "القليوبية",  new[]{1,2,3}  ),
            // الإسكندرية
            (4, 23,  "المنتزه",             "الإسكندرية", new[]{1,2,3,4}),
            (4, 24,  "سيدي جابر",           "الإسكندرية", new[]{1,2,3,4}),
            (4, 25,  "وسط الإسكندرية",      "الإسكندرية", new[]{1,2,3,4}),
            (4, 26,  "العجمي",              "الإسكندرية", new[]{1,2,3}  ),
            (4, 27,  "برج العرب",           "الإسكندرية", new[]{1,2,4}  ),
            (4, 28,  "ستانلي",              "الإسكندرية", new[]{1,3,4}  ),
            // البحيرة
            (5, 29,  "دمنهور",              "البحيرة",    new[]{1,2,3,4}),
            (5, 30,  "كفر الدوار",          "البحيرة",    new[]{1,2,3}  ),
            (5, 31,  "إيتاي البارود",       "البحيرة",    new[]{1,2}    ),
            (5, 32,  "رشيد",                "البحيرة",    new[]{2,4}    ),
            // الغربية
            (6, 33,  "طنطا",                "الغربية",    new[]{1,2,3,4}),
            (6, 34,  "المحلة الكبرى",       "الغربية",    new[]{1,2,3,4}),
            (6, 35,  "كفر الزيات",          "الغربية",    new[]{1,2,3}  ),
            (6, 36,  "سمنود",               "الغربية",    new[]{1,2}    ),
            // المنوفية
            (7, 37,  "شبين الكوم",          "المنوفية",   new[]{1,2,3,4}),
            (7, 38,  "منوف",                "المنوفية",   new[]{1,2,3}  ),
            (7, 39,  "أشمون",               "المنوفية",   new[]{1,2}    ),
            (7, 40,  "قويسنا",              "المنوفية",   new[]{2,4}    ),
            // الدقهلية
            (8, 41,  "المنصورة",            "الدقهلية",   new[]{1,2,3,4}),
            (8, 42,  "طلخا",                "الدقهلية",   new[]{1,2,3,4}),
            (8, 43,  "ميت غمر",             "الدقهلية",   new[]{1,2,3}  ),
            (8, 44,  "المنزلة",             "الدقهلية",   new[]{1,2}    ),
            // الشرقية
            (9, 45,  "الزقازيق",            "الشرقية",    new[]{1,2,3,4}),
            (9, 46,  "العاشر من رمضان",     "الشرقية",    new[]{1,2,3,4}),
            (9, 47,  "بلبيس",               "الشرقية",    new[]{1,2,3}  ),
            (9, 48,  "أبو كبير",            "الشرقية",    new[]{1,2}    ),
            // كفر الشيخ
            (10, 49, "كفر الشيخ",           "كفر الشيخ",  new[]{1,2,3,4}),
            (10, 50, "دسوق",                "كفر الشيخ",  new[]{1,2,3}  ),
            (10, 51, "فوه",                 "كفر الشيخ",  new[]{1,2}    ),
            // دمياط
            (11, 52, "دمياط",               "دمياط",      new[]{1,2,3,4}),
            (11, 53, "رأس البر",            "دمياط",      new[]{1,2,3}  ),
            (11, 54, "فارسكور",             "دمياط",      new[]{1,2}    ),
            // الإسماعيلية
            (12, 55, "الإسماعيلية",         "الإسماعيلية",new[]{1,2,3,4}),
            (12, 56, "القنطرة شرق",         "الإسماعيلية",new[]{1,2,3}  ),
            (12, 57, "فايد",                "الإسماعيلية",new[]{1,2}    ),
            // بورسعيد
            (13, 58, "حي الشرق",            "بورسعيد",    new[]{1,2,3,4}),
            (13, 59, "حي الغرب",            "بورسعيد",    new[]{1,2,3}  ),
            (13, 60, "بور فؤاد",            "بورسعيد",    new[]{1,2}    ),
            // السويس
            (14, 61, "حي الأربعين",         "السويس",     new[]{1,2,3,4}),
            (14, 62, "حي عتاقة",            "السويس",     new[]{1,2,3}  ),
            (14, 63, "حي فيصل السويس",      "السويس",     new[]{1,2}    ),
            // شمال سيناء
            (15, 64, "العريش",              "شمال سيناء", new[]{1,2,3,4}),
            (15, 65, "الشيخ زويد",          "شمال سيناء", new[]{1,2,3}  ),
            (15, 66, "رفح",                 "شمال سيناء", new[]{1,2}    ),
            // جنوب سيناء
            (16, 67, "شرم الشيخ",           "جنوب سيناء", new[]{1,2,3,4}),
            (16, 68, "دهب",                 "جنوب سيناء", new[]{1,2,3}  ),
            (16, 69, "طور سيناء",           "جنوب سيناء", new[]{1,2}    ),
            // الفيوم
            (17, 70, "الفيوم",              "الفيوم",     new[]{1,2,3,4}),
            (17, 71, "إبشواي",              "الفيوم",     new[]{1,2,3}  ),
            (17, 72, "سنورس",               "الفيوم",     new[]{1,2}    ),
            // بني سويف
            (18, 73, "بني سويف",            "بني سويف",   new[]{1,2,3,4}),
            (18, 74, "الفشن",               "بني سويف",   new[]{1,2,3}  ),
            (18, 75, "ناصر",                "بني سويف",   new[]{1,2}    ),
            // المنيا
            (19, 76, "المنيا",              "المنيا",     new[]{1,2,3,4}),
            (19, 77, "أبو قرقاص",           "المنيا",     new[]{1,2,3}  ),
            (19, 78, "ملوي",                "المنيا",     new[]{1,2,3}  ),
            (19, 79, "سمالوط",              "المنيا",     new[]{1,2}    ),
            (19, 80, "بني مزار",            "المنيا",     new[]{1,3}    ),
            // أسيوط
            (20, 81, "أسيوط",               "أسيوط",      new[]{1,2,3,4}),
            (20, 82, "ديروط",               "أسيوط",      new[]{1,2,3}  ),
            (20, 83, "منفلوط",              "أسيوط",      new[]{1,2,3}  ),
            (20, 84, "القوصية",             "أسيوط",      new[]{1,2}    ),
            // سوهاج
            (21, 85, "سوهاج",               "سوهاج",      new[]{1,2,3,4}),
            (21, 86, "طهطا",                "سوهاج",      new[]{1,2,3}  ),
            (21, 87, "جرجا",                "سوهاج",      new[]{1,2}    ),
            (21, 88, "أخميم",               "سوهاج",      new[]{1,3}    ),
            // قنا
            (22, 89, "قنا",                 "قنا",        new[]{1,2,3,4}),
            (22, 90, "قوص",                 "قنا",        new[]{1,2,3}  ),
            (22, 91, "نجع حمادي",           "قنا",        new[]{1,2,3,4}),
            // الأقصر
            (23, 92, "الأقصر",              "الأقصر",     new[]{1,2,3,4}),
            (23, 93, "إسنا",                "الأقصر",     new[]{1,2,3}  ),
            // أسوان
            (24, 94, "أسوان",               "أسوان",      new[]{1,2,3,4}),
            (24, 95, "كوم أمبو",            "أسوان",      new[]{1,2,3}  ),
            (24, 96, "إدفو",                "أسوان",      new[]{1,2}    ),
            // مطروح
            (25, 97, "مرسى مطروح",          "مطروح",      new[]{1,2,3,4}),
            (25, 98, "العلمين",             "مطروح",      new[]{1,2,3}  ),
            (25, 99, "سيوة",                "مطروح",      new[]{1,2}    ),
            // البحر الأحمر
            (26,100, "الغردقة",             "البحر الأحمر",new[]{1,2,3,4}),
            (26,101, "سفاجا",               "البحر الأحمر",new[]{1,2,3}  ),
            (26,102, "مرسى علم",            "البحر الأحمر",new[]{1,2}    ),
            // الوادي الجديد
            (27,103, "الخارجة",             "الوادي الجديد",new[]{1,2,3,4}),
            (27,104, "الداخلة",             "الوادي الجديد",new[]{1,2}    ),
            (27,105, "الفرافرة",            "الوادي الجديد",new[]{1,3}    ),
        };

        foreach (var row in rows)
        {
            foreach (int op in row.Ops)
            {
                int idx = (id + op + row.G) % statuses.Length;
                double dist = Math.Round(1.2 + (id % 15) * 0.5, 1);
                list.Add(new Branch
                {
                    Id            = id++,
                    OperatorId    = op,
                    GovernorateId = row.G,
                    DistrictId    = row.D,
                    NameAr        = opNames[op] + " - " + row.DN,
                    Area          = row.DN,
                    Address       = "شارع رئيسي، " + row.DN + "، محافظة " + row.GN,
                    DistanceKm    = dist,
                    Status        = statuses[idx],
                    QueueCount    = queues[idx],
                    WaitTime      = waits[idx],
                    Rating        = ratings[idx]
                });
            }
        }

        mb.Entity<Branch>().HasData(list);
    }
}
