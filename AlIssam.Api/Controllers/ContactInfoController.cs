using Microsoft.AspNetCore.Mvc;
using AlIssam.DataAccessLayer.Entities;
using AlIssam.DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace AlIssam.API.Controllers
{
    public class ContactInfoController : ControllerBase
    {
        private readonly AlIssamDbContext _context;

        public ContactInfoController(AlIssamDbContext context)
        {
            _context = context;
        }

        // GET: ContactInfo (Always returns the record with Id = 1)
        [HttpGet("/api/contact/info")]
        public async Task<IActionResult> GetContactInfo()
        {
            // Fetch the ContactInfo record with Id = 1
            var contactInfo = await _context.ContactInfos
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (contactInfo == null)
                return NotFound("Contact information not found.");
            

            return Ok(contactInfo);
        }

        // UPDATE: ContactInfo (Updates the record with Id = 1)
        [HttpPost("/api/contact")]
        public async Task<IActionResult> UpdateContactInfo([FromBody] ContactInfo updatedContactInfo)
        {
            if (updatedContactInfo == null )
                return BadRequest("Invalid data or ID.");

            // Fetch the existing ContactInfo record with Id = 1
            var existingContactInfo = await _context.ContactInfos
                .FirstOrDefaultAsync(c => c.Id == 1);

            if (existingContactInfo == null)
            {
                return NotFound("Contact information not found.");
            }

            // Update all fields
            existingContactInfo.Phone_Number = updatedContactInfo.Phone_Number;
            existingContactInfo.Location_Ar = updatedContactInfo.Location_Ar;
            existingContactInfo.Location_En = updatedContactInfo.Location_En;
            existingContactInfo.Instagram = updatedContactInfo.Instagram;
            existingContactInfo.WhatsApp = updatedContactInfo.WhatsApp;
            existingContactInfo.TikTok = updatedContactInfo.TikTok;

            // Save changes to the database
            _context.ContactInfos.Update(existingContactInfo);
            await _context.SaveChangesAsync();

            return Ok(existingContactInfo);
        }
    }
}
