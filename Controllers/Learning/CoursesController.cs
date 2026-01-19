using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Learning;

namespace UniStart.Controllers.Learning;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CoursesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/courses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
    {
        var courses = await _context.Courses
            .Where(c => c.IsActive)
            .OrderBy(c => c.OrderIndex)
            .ToListAsync();
            
        return Ok(courses);
    }

    // GET: api/courses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Course>> GetCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        return Ok(course);
    }
}
