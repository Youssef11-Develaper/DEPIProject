using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using RegisistryV2.ViewModel.Rating;
using System.Data;

namespace RegisistryV2.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly IRatingRepository _ratingRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingController(
            IRatingRepository ratingRepository,
            IAppointmentRepository appointmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _ratingRepository = ratingRepository;
            _appointmentRepository = appointmentRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int appointmentId)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetWithDetailsAsync(appointmentId);

            if (appointment == null || appointment.UserId != userId
                || appointment.Status != AppointmentStatus.Completed)
                return NotFound();

            var alreadyRated = await _ratingRepository.HasRatedAsync(appointmentId);
            if (alreadyRated)
            {
                TempData["Error"] = "قيمت الموعد ده قبل كده";
                return RedirectToAction("MyAppointments", "Appointment");
            }

            ViewBag.Appointment = appointment;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RatingViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            var appointment = await _appointmentRepository.GetWithDetailsAsync(model.AppointmentId);

            if (appointment == null || appointment.UserId != userId
                || appointment.Status != AppointmentStatus.Completed)
                return NotFound();

            var rating = new Rating
            {
                UserId = userId,
                AppointmentId = model.AppointmentId,
                BranchId = appointment.BranchId,
                Stars = model.Stars,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            await _ratingRepository.AddAsync(rating);
            await _ratingRepository.SaveChangesAsync();

            TempData["Success"] = "شكراً على تقييمك!";
            return RedirectToAction("MyAppointments", "Appointment");
        }

        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> BranchRatings(int branchId)
        {
            var ratings = await _ratingRepository.GetBranchRatingsAsync(branchId);
            var average = await _ratingRepository.GetBranchAverageAsync(branchId);

            ViewBag.Average = average;
            return View(ratings);
        }
    }
}
