using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskAi.Api.Data;
using TaskAi.Api.Models;
using TaskAi.Api.Services; // Ensure this matches your service namespace
using UglyToad.PdfPig;

namespace TaskAi.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AiService _aiService; // Inject the AI Service

        public TasksController(AppDbContext context, AiService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpGet("ai-summary")]
        public async Task<IActionResult> GetAiSummary()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
            var userId = Guid.Parse(userIdString);

            var now = DateTime.UtcNow.Date;
            var nextWeek = now.AddDays(7);

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .ToListAsync();

            // Filter data based on your requirements
            var todayTasks = tasks.Where(t => t.DueDate?.Date == now).Select(t => t.Title).ToList();
            var upcomingTasks = tasks.Where(t => t.DueDate?.Date > now && t.DueDate?.Date <= nextWeek).Select(t => t.Title).ToList();
            var total = tasks.Count;

            // Create context string for the AI
            string context = $"User has {total} total tasks. " +
                             $"Tasks due today: {(todayTasks.Any() ? string.Join(", ", todayTasks) : "None")}. " +
                             $"Tasks due in next 7 days: {(upcomingTasks.Any() ? string.Join(", ", upcomingTasks) : "None")}.";

            // Call the real AI Service
            var summaryText = await _aiService.GetConversationalSummary(context);

            return Ok(new { summary = summaryText });
        }

        [HttpPost("extract-info")]
        public async Task<IActionResult> ExtractInfo(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            string extractedText = "";

            try
            {
                // Handle PDF files
                if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = file.OpenReadStream();
                    using var pdf = PdfDocument.Open(stream);
                    foreach (var page in pdf.GetPages())
                    {
                        extractedText += page.Text;
                    }
                }
                else // Handle Text/EML/Log files
                {
                    using var reader = new StreamReader(file.OpenReadStream());
                    extractedText = await reader.ReadToEndAsync();
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                    return BadRequest("No readable text found in document.");

                // Use AI to extract structured data from the text
                var result = await _aiService.ExtractTasksFromText(extractedText);

                // Note: The result from AI is a string; we return it to the frontend 
                // which expects an object with a 'tasks' array.
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }

        // --- Existing standard CRUD methods below (GetAll, GetById, Create, Update, Delete) ---

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = Guid.Parse(userIdString);
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = Guid.Parse(userIdString);
            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            return Ok(task);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var task = new TaskItem
            {
                Id = Guid.NewGuid(),
                UserId = Guid.Parse(userIdString),
                Title = request.Title,
                Description = request.Description,
                DueDate = DateTime.SpecifyKind(request.DueDate ?? DateTime.UtcNow, DateTimeKind.Utc),
                Status = request.Status ?? "To Do",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, TaskItem request)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = Guid.Parse(userIdString);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.Status = request.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var userId = Guid.Parse(userIdString);
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping() => Ok("API is running");
    }
}