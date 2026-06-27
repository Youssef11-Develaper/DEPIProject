using RegisistryV2.Models;

namespace RegisistryV2.ViewModel.BranchAdmin
{
    public class BranchDetailsViewModel
    {
        public Branch Branch { get; set; }

        public List<BranchSchedule> Schedules { get; set; }

        public List<BranchHoliday> Holidays { get; set; }

        public List<ServiceUnavailability> Unavailabilities { get; set; }

        public List<ServiceType> Services { get; set; }
    }
}
