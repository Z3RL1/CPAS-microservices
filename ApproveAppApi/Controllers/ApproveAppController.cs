﻿using ApproveAppApi.Data;
using ApproveAppApi.Models.Dto;
using ApproveAppApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermitApplicationWebApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApproveAppApi.Controllers
{
    [Route("approveapi/approve")]
    [ApiController]
    public class ApproveAppController : ControllerBase
    {
        private readonly ApproveAPIDbContext _approveAPIDbContext;
        private readonly PermitService _permitService;


        public ApproveAppController(ApproveAPIDbContext approveAPIDbContext, PermitService permitService)
        {
            _approveAPIDbContext = approveAPIDbContext;
            _permitService = permitService;
        }

        // POST api/approve/
        [HttpPost]
        public async Task<ActionResult> ApproveApplication([FromBody] PermitInfoDto permitInfo)
        {

            var getUserID = await _approveAPIDbContext.Users.FindAsync(permitInfo.UserId);
            if (getUserID == null)
            {
                return NotFound();
            }

            var existingPermit = await _approveAPIDbContext.Permits.Where(u => u.PermitId == permitInfo.Id).FirstOrDefaultAsync();

            if (existingPermit != null)
            {

                var approvedPermit = _permitService.MapPermitInfoDtoToEntity(permitInfo);

                existingPermit.Status = "Approved";
                existingPermit.UpdatedBy = getUserID.Username;
                existingPermit.DateTimeUpdated = DateTime.Now;

                await _approveAPIDbContext.SaveChangesAsync();
            }

            return Ok("Permit is approved successfully.");

        }


        [HttpPost("rejectpermit")]
        public async Task<ActionResult> RejectApplication([FromBody] PermitInfoDto permitInfo)
        {
            // Validate user
            var getUserID = await _approveAPIDbContext.Users.FindAsync(permitInfo.UserId);
            if (getUserID == null)
            {
                return NotFound("User not found.");
            }

            // Find the permit
            var existingPermit = await _approveAPIDbContext.Permits
                .Where(u => u.PermitId == permitInfo.Id)
                .FirstOrDefaultAsync();

            if (existingPermit == null)
            {
                return NotFound("Permit not found.");
            }

            // Update status to Rejected
            existingPermit.Status = "Rejected";
            existingPermit.UpdatedBy = getUserID.Username;
            existingPermit.DateTimeUpdated = DateTime.Now;

            // Save changes to the database
            await _approveAPIDbContext.SaveChangesAsync();

            return Ok("Permit is rejected successfully.");
        }


        // POST api/approve/{id}
        //[HttpPost("{id}")]
        //public async Task<ActionResult> ApproveApplication(long id, string approver)
        //{

        //    var existingPermit = await _approveAPIDbContext.Permit.FindAsync(id);

        //    if (existingPermit != null)
        //    {
        //        existingPermit.Status = "Approved";
        //        existingPermit.UpdatedBy = approver;
        //        existingPermit.DateTimeUpdated = DateTime.Now;

        //        await _approveAPIDbContext.SaveChangesAsync();
        //    }

        //    return Ok("Permit is approved successfully.");

        //}

        // GET: api/approve/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permit>> GetPermit(long id)
        {
            if (_approveAPIDbContext.Permits == null)
            {
                return NotFound("Unable to find the permit");
            }
            var permit = await _approveAPIDbContext.Permits.FindAsync(id);

            if (permit == null)
            {
                return NotFound("Unable to find the permit");
            }

            return permit;
        }


    }
}
