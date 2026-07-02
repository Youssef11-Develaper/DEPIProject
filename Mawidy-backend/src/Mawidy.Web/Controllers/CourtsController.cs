using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mawidy.Application.Interfaces;
using Mawidy.Web.ViewModels.Courts;
using Mawidy.Application.Features.Courts.Queries;
using Mawidy.Application.DTOs.Courts;

namespace Mawidy.Web.Controllers;

public class CourtsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public CourtsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Debug DB status
        var dbCount = await _context.Courts.CountAsync();
        Console.WriteLine($"[DEBUG] Index() - DB Courts Count: {dbCount}");

        // 1. Fetch Courts from DB via MediatR Query
        var courtDtos = await _mediator.Send(new GetCourtsQuery("الكل", null));
        Console.WriteLine($"[DEBUG] Index() - Query CourtDtos Count: {courtDtos.Count}");
        var courts = courtDtos.Select(MapToCourtViewModel).ToList();

        // 2. Fetch Cases from DB via MediatR TrackCaseQuery
        var cases = new Dictionary<string, CaseViewModel>();
        
        var caseNumbers = new[] { "2025/4821", "2024/1190" };
        foreach (var num in caseNumbers)
        {
            var caseDto = await _mediator.Send(new TrackCaseQuery(num));
            if (caseDto != null)
            {
                cases[num] = MapToCaseViewModel(caseDto);
            }
        }

        // 3. Build Page Model
        var viewModel = new CourtsPageViewModel
        {
            Courts = courts,
            Cases = cases,
            Faqs = GetFaqsData()
        };

        return View(viewModel);
    }

    private static CourtViewModel MapToCourtViewModel(CourtDto dto)
    {
        return new CourtViewModel
        {
            Name = dto.Name,
            Type = dto.Type,
            Icon = dto.Type switch
            {
                "مدنية" => "🏛️",
                "جنائية" => "🔨",
                "أسرة" => "👨‍👩‍👧",
                "تجارية" => "💼",
                "استئناف" => "📋",
                _ => "🏛️"
            },
            Addr = dto.Address,
            Dist = $"{dto.DistanceKm:F1} كم",
            Status = dto.Status,
            Queue = dto.QueueCount,
            Wait = dto.WaitTime,
            Rooms = dto.TotalRooms,
            Sessions = dto.SessionsToday,
            Phone = dto.Phone
        };
    }

    private static CaseViewModel MapToCaseViewModel(LegalCaseDto dto)
    {
        return new CaseViewModel
        {
            Title = dto.Title,
            Type = dto.Type,
            Steps = dto.Timeline.Select(t => new CaseStepViewModel
            {
                Icon = t.Icon,
                Status = t.Status,
                Title = t.Title,
                Date = t.Date,
                Note = t.Note ?? string.Empty
            }).ToList()
        };
    }

    private static List<FaqViewModel> GetFaqsData()
    {
        return
        [
            new() { Icon = "❓", Q = "هل الحجز على منصة مـوعـدي مجاني؟", A = "نعم، الحجز والمتابعة على منصة مـوعـدي مجانية تماماً. الرسوم الوحيدة هي الرسوم القضائية الرسمية للمحكمة والتي تُدفع مباشرة للمحكمة." },
            new() { Icon = "⏰", Q = "كم وقت يستغرق تأكيد الحجز؟", A = "يصلك تأكيد الحجز فوراً عبر SMS في غضون ثوانٍ من إتمام الحجز. كود QR يكون جاهزاً للعرض مباشرة على الهاتف." },
            new() { Icon = "📋", Q = "ماذا لو لم أحضر في الموعد المحدد؟", A = "إذا لم تتمكن من الحضور، يُفضل إلغاء الحجز قبل الموعد بـ 24 ساعة على الأقل حتى يتاح الموعد لشخص آخر. يمكنك إعادة الحجز مرة أخرى مجاناً." },
            new() { Icon = "🔄", Q = "هل يمكنني تغيير أو تعديل الموعد؟", A = "نعم، يمكنك تعديل أو إلغاء الحجز من خلال صفحة \"مواعيدي\" في منصة مـوعـدي حتى 12 ساعة قبل الموعد." },
            new() { Icon = "👨‍⚖️", Q = "هل أحتاج لمحامٍ لحجز الجلسة؟", A = "لا، يمكنك حجز جلسة بدون محامٍ في كثير من القضايا. لكن يُنصح بالاستعانة بمحامٍ خاصة في القضايا المعقدة. المنصة تتيح قائمة بمحامين معتمدين." },
            new() { Icon = "📱", Q = "هل يمكن متابعة أكثر من قضية؟", A = "نعم، بعد تسجيل الدخول يمكنك متابعة عدد غير محدود من القضايا وإضافتها لصفحة \"متابعة قضاياي\" في حسابك." },
            new() { Icon = "💳", Q = "ما هي طرق دفع الرسوم القضائية؟", A = "تتيح المنصة دفع الرسوم إلكترونياً بالبطاقات البنكية وبطاقات المحافظ الرقمية (فودافون كاش، اتصالات كاش). يمكن أيضاً الدفع نقداً في صندوق المحكمة." },
            new() { Icon = "🔒", Q = "هل بياناتي الشخصية آمنة؟", A = "نعم، جميع البيانات الشخصية وبيانات القضايا مشفرة بالكامل (AES-256) وفق أعلى معايير الأمان. لا تُشارك البيانات مع أي طرف ثالث دون موافقتك." },
        ];
    }

    [HttpGet]
    public async Task<IActionResult> Filter(string type, string search)
    {
        var courtDtos = await _mediator.Send(new GetCourtsQuery(type, search));
        var courts = courtDtos.Select(MapToCourtViewModel).ToList();
        return PartialView("_CourtsGrid", courts);
    }

    [HttpGet]
    public async Task<IActionResult> Track(string caseNum)
    {
        // Try to query the database first
        var caseDto = await _mediator.Send(new TrackCaseQuery(caseNum));
        if (caseDto == null)
        {
            return PartialView("_CaseNotFound");
        }
        var viewModel = MapToCaseViewModel(caseDto);
        return PartialView("_CaseTimeline", viewModel);
    }
}
