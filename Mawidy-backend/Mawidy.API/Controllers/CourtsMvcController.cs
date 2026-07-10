using Mawidy.Domain.Entities.Hospitals;
using Mawidy.Domain.Entities.Banks;
using Mawidy.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mawidy.API.ViewModels.Courts;
using Mawidy.Application.Features.Courts.Queries;
using Mawidy.Application.DTOs.Courts;

namespace Mawidy.API.Controllers;

[Route("Courts")]
public class CourtsMvcController : Controller
{
    private readonly IMediator _mediator;

    public CourtsMvcController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var courtDtos = await _mediator.Send(new GetCourtsQuery("الكل", null));
        var courts = courtDtos.Select(MapToCourtViewModel).ToList();

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

        var viewModel = new CourtsPageViewModel
        {
            Courts = courts,
            Cases = cases,
            Faqs = GetFaqsData()
        };

        return View(viewModel);
    }

    [HttpGet("Filter")]
    public async Task<IActionResult> Filter(string type, string search)
    {
        var courtDtos = await _mediator.Send(new GetCourtsQuery(type, search));
        var courts = courtDtos.Select(MapToCourtViewModel).ToList();
        return PartialView("_CourtsGrid", courts);
    }

    [HttpGet("Track")]
    public async Task<IActionResult> Track(string caseNum)
    {
        var caseDto = await _mediator.Send(new TrackCaseQuery(caseNum));
        if (caseDto == null)
        {
            return PartialView("_CaseNotFound");
        }
        var viewModel = MapToCaseViewModel(caseDto);
        return PartialView("_CaseTimeline", viewModel);
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
}
