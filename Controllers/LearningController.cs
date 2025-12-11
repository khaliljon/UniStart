using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LearningController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LearningController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получить все курсы
    /// </summary>
    [HttpGet("courses")]
    public async Task<ActionResult> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Subjects)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Year != null ? c.Year : 0)
            .ThenBy(c => c.Title)
            .Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                c.Icon,
                c.CoverImageUrl,
                c.Year,
                c.Direction,
                SubjectsCount = c.Subjects.Count,
                Subjects = c.Subjects
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(courses);
    }

    /// <summary>
    /// Получить курс по ID с полной иерархией
    /// </summary>
    [HttpGet("courses/{courseId}")]
    public async Task<ActionResult> GetCourseWithHierarchy(int courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Subjects)
                .ThenInclude(s => s.LearningModules)
                    .ThenInclude(m => m.Competencies)
                        .ThenInclude(comp => comp.Topics)
                            .ThenInclude(t => t.Theory)
            .Include(c => c.Subjects)
                .ThenInclude(s => s.LearningModules)
                    .ThenInclude(m => m.Competencies)
                        .ThenInclude(comp => comp.Topics)
                            .ThenInclude(t => t.PracticeQuiz)
            .Include(c => c.Subjects)
                .ThenInclude(s => s.LearningModules)
                    .ThenInclude(m => m.CaseStudyQuiz)
            .Include(c => c.Subjects)
                .ThenInclude(s => s.LearningModules)
                    .ThenInclude(m => m.ModuleFinalQuiz)
            .Where(c => c.Id == courseId && c.IsActive)
            .FirstOrDefaultAsync();

        if (course == null)
            return NotFound("Курс не найден");

        var result = new
        {
            course.Id,
            course.Title,
            course.Description,
            course.Icon,
            course.CoverImageUrl,
            course.Year,
            course.Direction,
            Subjects = course.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    Modules = s.LearningModules
                        .Where(m => m.IsActive)
                        .OrderBy(m => m.OrderIndex)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.Description,
                            m.Icon,
                            m.OrderIndex,
                            m.EntQuestionCount,
                            Competencies = m.Competencies
                                .Where(c => c.IsActive)
                                .OrderBy(c => c.OrderIndex)
                                .Select(c => new
                                {
                                    c.Id,
                                    c.Title,
                                    c.Description,
                                    c.Icon,
                                    c.OrderIndex,
                                    Topics = c.Topics
                                        .Where(t => t.IsActive)
                                        .OrderBy(t => t.OrderIndex)
                                        .Select(t => new
                                        {
                                            t.Id,
                                            t.Title,
                                            t.Description,
                                            t.Icon,
                                            t.OrderIndex,
                                            HasTheory = t.Theory != null,
                                            TheoryId = t.Theory != null ? t.Theory.Id : (int?)null,
                                            HasPracticeQuiz = t.PracticeQuiz != null,
                                            PracticeQuizId = t.PracticeQuizId,
                                            PracticeQuizTitle = t.PracticeQuiz != null ? t.PracticeQuiz.Title : null
                                        })
                                        .ToList()
                                })
                                .ToList(),
                            HasCaseStudy = m.CaseStudyQuiz != null,
                            CaseStudyQuizId = m.CaseStudyQuizId,
                            CaseStudyQuizTitle = m.CaseStudyQuiz != null ? m.CaseStudyQuiz.Title : null,
                            HasModuleFinalQuiz = m.ModuleFinalQuiz != null,
                            ModuleFinalQuizId = m.ModuleFinalQuizId,
                            ModuleFinalQuizTitle = m.ModuleFinalQuiz != null ? m.ModuleFinalQuiz.Title : null
                        })
                        .ToList()
                })
                .ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить предмет с полной иерархией (standalone, без курса)
    /// </summary>
    [HttpGet("subjects/{subjectId}")]
    public async Task<ActionResult> GetSubjectWithHierarchy(int subjectId)
    {
        var subject = await _context.Subjects
            .Include(s => s.LearningModules)
                .ThenInclude(m => m.Competencies)
                    .ThenInclude(comp => comp.Topics)
                        .ThenInclude(t => t.Theory)
            .Include(s => s.LearningModules)
                .ThenInclude(m => m.Competencies)
                    .ThenInclude(comp => comp.Topics)
                        .ThenInclude(t => t.PracticeQuiz)
            .Include(s => s.LearningModules)
                .ThenInclude(m => m.CaseStudyQuiz)
            .Include(s => s.LearningModules)
                .ThenInclude(m => m.ModuleFinalQuiz)
            .Where(s => s.Id == subjectId && s.IsActive)
            .FirstOrDefaultAsync();

        if (subject == null)
            return NotFound("Предмет не найден");

        var result = new
        {
            subject.Id,
            subject.Name,
            subject.Description,
            Modules = subject.LearningModules
                .Where(m => m.IsActive)
                .OrderBy(m => m.OrderIndex)
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Description,
                    m.Icon,
                    m.OrderIndex,
                    m.EntQuestionCount,
                    Competencies = m.Competencies
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.OrderIndex)
                        .Select(c => new
                        {
                            c.Id,
                            c.Title,
                            c.Description,
                            c.Icon,
                            c.OrderIndex,
                            Topics = c.Topics
                                .Where(t => t.IsActive)
                                .OrderBy(t => t.OrderIndex)
                                .Select(t => new
                                {
                                    t.Id,
                                    t.Title,
                                    t.Description,
                                    t.Icon,
                                    t.OrderIndex,
                                    HasTheory = t.Theory != null,
                                    TheoryId = t.Theory != null ? t.Theory.Id : (int?)null,
                                    HasPracticeQuiz = t.PracticeQuiz != null,
                                    PracticeQuizId = t.PracticeQuizId,
                                    PracticeQuizTitle = t.PracticeQuiz != null ? t.PracticeQuiz.Title : null
                                })
                                .ToList()
                        })
                        .ToList(),
                    HasCaseStudy = m.CaseStudyQuiz != null,
                    CaseStudyQuizId = m.CaseStudyQuizId,
                    CaseStudyQuizTitle = m.CaseStudyQuiz != null ? m.CaseStudyQuiz.Title : null,
                    HasModuleFinalQuiz = m.ModuleFinalQuiz != null,
                    ModuleFinalQuizId = m.ModuleFinalQuizId,
                    ModuleFinalQuizTitle = m.ModuleFinalQuiz != null ? m.ModuleFinalQuiz.Title : null
                })
                .ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить теорию по ID
    /// </summary>
    [HttpGet("theory/{theoryId}")]
    public async Task<ActionResult> GetTheory(int theoryId)
    {
        var theory = await _context.TheoryContents
            .Include(t => t.LearningTopic)
                .ThenInclude(lt => lt.LearningCompetency)
                    .ThenInclude(lc => lc.LearningModule)
                        .ThenInclude(lm => lm.Subject)
            .Where(t => t.Id == theoryId)
            .FirstOrDefaultAsync();

        if (theory == null)
            return NotFound("Теория не найдена");

        var result = new
        {
            theory.Id,
            theory.Title,
            theory.Content,
            theory.CoverImageUrl,
            theory.EstimatedReadTimeMinutes,
            theory.AdditionalResources,
            Topic = new
            {
                theory.LearningTopic.Id,
                theory.LearningTopic.Title,
                Competency = new
                {
                    theory.LearningTopic.LearningCompetency.Id,
                    theory.LearningTopic.LearningCompetency.Title,
                    Module = new
                    {
                        theory.LearningTopic.LearningCompetency.LearningModule.Id,
                        theory.LearningTopic.LearningCompetency.LearningModule.Title,
                        Subject = new
                        {
                            theory.LearningTopic.LearningCompetency.LearningModule.Subject.Id,
                            theory.LearningTopic.LearningCompetency.LearningModule.Subject.Name
                        }
                    }
                }
            }
        };

        return Ok(result);
    }

    /// <summary>
    /// Получить все предметы с модулями (для списка)
    /// </summary>
    [HttpGet("subjects")]
    public async Task<ActionResult> GetSubjects()
    {
        var subjects = await _context.Subjects
            .Include(s => s.LearningModules)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                ModulesCount = s.LearningModules.Count(m => m.IsActive),
                HasHierarchy = s.LearningModules.Any(m => m.IsActive)
            })
            .ToListAsync();

        return Ok(subjects);
    }
}

