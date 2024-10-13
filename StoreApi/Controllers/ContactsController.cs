using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApi.Models;
using StoreApi.Services;

namespace StoreApi.Controllers;

[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;


    public ContactsController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet("Subjects")]
    public IActionResult GetSubjects()
    {
        var listSubject = _context.Subjects.ToList();
        return Ok(listSubject);
    }

    [HttpGet]
    public IActionResult GetContacts(int? page)
    {
        if (page == null || page < 1)
        {
            page = 1;
        }

        int pageSize = 5;
        int totalPages = 0;
        decimal count = _context.Contacts.Count();
        totalPages = (int)Math.Ceiling(count / pageSize);
        var contacts = _context.Contacts
            .Include(c => c.Subject)
            .OrderBy(c => c.Id)
            .Skip((int) (page - 1) * pageSize)
            .Take(pageSize )
            .ToList();

        var response = new
        {
            Contacts = contacts,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetContact(int id)
    {
        var contact = _context.Contacts.Include(c => c.Subject).FirstOrDefault(c => c.Id == id);
        if (contact == null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    [HttpPost]
    public IActionResult CreateContact([FromBody]ContactDto contactDto)
    {
        var subject = _context.Subjects.Find(contactDto.SubjectId);
        if (subject == null)
        {
            ModelState.AddModelError("Subject" , "Please Enter a valid Subject!");
            return BadRequest(ModelState);
        }
        var contact = new Contact
        {
            Email = contactDto.Email,
            FirstName = contactDto.FirstName,
            LastName = contactDto.LastName,
            Subject = subject,
            Message = contactDto.Message,
            Phone = contactDto.Phone,
            CreatedAt = DateTime.Now
        };
        _context.Add(contact);
        _context.SaveChanges();
        var header = contact.FirstName + " " + contact.LastName;
        _emailService.SendEmail(header  ,contact.Message);
        return Ok(contact);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateContact(int id, ContactDto contactDto)
    {
        var subject = _context.Subjects.Find(id);
        var contact = _context.Contacts.Find(id);
        if (subject == null)
        {
            ModelState.AddModelError("Subject" , "Please Enter a valid Subject!");
            return BadRequest(ModelState);
        }
        if (contact == null)
        {
            return NotFound();
        }

        contact.FirstName = contactDto.FirstName ?? contact.FirstName;
        contact.LastName = contactDto.LastName?? contact.LastName;
        contact.Email = contactDto.Email ?? contact.Email;
        contact.Message = contactDto.Message?? contact.Message;
        contact.Phone = contactDto.Phone?? contact.Phone;
        contact.Subject = subject;
        _context.SaveChanges();
        return Ok(contact);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteContact(int id)
    {
        // var contact = _context.Contacts.Find(id);
        // if (contact == null)
        // {
        //     return NotFound();
        // }
        // _context.Contacts.Remove(contact);
        // _context.SaveChanges();
        // return Ok();
        /*
         */
        // var contact = new Contact() { Id = id };
        // _context.Contacts.Remove(contact);
        // _context.SaveChanges();
        // return Ok();
        /*
         */
        try
        {
            var contact = new Contact() { Id = id, Subject = new Subject()};
            _context.Contacts.Remove(contact);
            _context.SaveChanges();
        }
        catch (Exception)
        {
            return NotFound();
        }

        return Ok();
    }
}