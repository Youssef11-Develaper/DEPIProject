using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegisistryV2.Data;
using RegisistryV2.Models;
using RegisistryV2.Repository.Interface;
using System.Data;

namespace RegisistryV2.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class AdminComplaintController : Controller
    {
        private readonly IComplaintRepository _complaintRepository;

        public AdminComplaintController(IComplaintRepository complaintRepository)
        {
            _complaintRepository = complaintRepository;
        }

        public async Task<IActionResult> Index(int? status)
        {
            var complaints = await _complaintRepository
                .GetAllWithDetailsAsync(status.HasValue ? (ComplaintStatus)status : null);
            return View(complaints);
        }

        [HttpPost]
        public async Task<IActionResult> Respond(int id, string adminResponse, ComplaintStatus status)
        {
            var complaint = await _complaintRepository.GetByIdAsync(id);
            if (complaint == null) return NotFound();

            complaint.AdminResponse = adminResponse;
            complaint.Status = status;

            _complaintRepository.Update(complaint);
            await _complaintRepository.SaveChangesAsync();

            TempData["Success"] = "تم الرد على الشكوى";
            return RedirectToAction("Index");
        }
    }
}
