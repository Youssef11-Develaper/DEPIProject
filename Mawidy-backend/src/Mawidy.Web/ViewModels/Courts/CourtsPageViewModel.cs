namespace Mawidy.Web.ViewModels.Courts;

public class CourtsPageViewModel
{
    public List<CourtViewModel> Courts { get; set; } = [];
    public Dictionary<string, CaseViewModel> Cases { get; set; } = [];
    public List<FaqViewModel> Faqs { get; set; } = [];
}
