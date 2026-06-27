using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using RegisistryV2.ViewModel;
using System.Diagnostics;

namespace RegisistryV2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBranchRepository _branchRepository;

        public HomeController(
            IRatingRepository ratingRepository,
            IAppointmentRepository appointmentRepository,
            IBranchRepository branchRepository)
        {
            _ratingRepository = ratingRepository;
            _appointmentRepository = appointmentRepository;
            _branchRepository = branchRepository;
        }

        public async Task<IActionResult> Index()
        {
            var ratings = await _ratingRepository.GetRecentPositiveRatingsAsync(4);
            var allRatings = await _ratingRepository.GetAllAsync();

            var viewModel = new HomeViewModel
            {
                AverageRating = allRatings.Any()
                    ? Math.Round(allRatings.Average(r => r.Stars), 1) : 0,
                TotalRatings = allRatings.Count(),
                TotalCompletedAppointments = (await _appointmentRepository
                    .GetAllAsync())
                    .Count(a => a.Status == AppointmentStatus.Completed),
                RecentRatings = ratings.Select(r => new RecentRatingViewModel
                {
                    UserName = MaskName(r.User.FullName),
                    Stars = r.Stars,
                    Comment = r.Comment,
                    ServiceName = r.Appointment.ServiceType.Name,
                    BranchName = r.Branch.Name,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return View(viewModel);
        }

        private string MaskName(string fullName)
        {
            var parts = fullName.Split(' ');
            return parts.Length >= 2
                ? $"{parts[0]} {parts[1][0]}."
                : parts[0];
        }
    }
}
