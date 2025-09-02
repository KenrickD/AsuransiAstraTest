using Domain.Models;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AsuransiAstraTest.Pages
{
    public class LandingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public LandingModel(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> OnPostUpdateUniversityAsync()
        {
            try
            {
                var universityId = Request.Form["universityId"];
                var name = Request.Form["name"];
                var website = Request.Form["website"];

                if (string.IsNullOrEmpty(universityId) || string.IsNullOrEmpty(name))
                {
                    return new JsonResult(new { success = false, error = "Required fields missing" });
                }

                var university = await _context.Universities.FindAsync(Guid.Parse(universityId));

                if (university == null)
                {
                    return new JsonResult(new { success = false, error = "University not found" });
                }

                university.Name = name;
                university.WebPages = website;

                _context.Universities.Update(university);
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public class UpdateUniversityRequest
        {
            public string UniversityId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Website { get; set; } = string.Empty;
        }

        public string Username { get; set; } = string.Empty;
        public List<University> Universities { get; set; } = new List<University>();

        public async Task OnGetAsync()
        {
            Username = TempData["Username"]?.ToString() ?? "User";
            TempData.Keep("Username"); 

            var universityCount = await _context.Universities.CountAsync();

            if (universityCount == 0)
            {
                await LoadUniversitiesFromApi();
            }

            Universities = await _context.Universities.ToListAsync();
        }

        private async Task LoadUniversitiesFromApi()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("http://universities.hipolabs.com/search?country=Indonesia");
                var universityData = JsonSerializer.Deserialize<List<UniversityApiResponse>>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (universityData != null)
                {
                    var universities = universityData.Take(50).Select(u => new University
                    {
                        UniversityId = Guid.NewGuid(),
                        Name = u.name,
                        WebPages = u.web_pages?.FirstOrDefault(),
                        Domains = string.Join(",", u.domains ?? new List<string>()),
                        CountryCode = u.alpha_two_code,
                        StateProvince = u.state_province
                    }).ToList();

                    await _context.Universities.AddRangeAsync(universities);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading universities from API: {ex.Message}");
            }
        }



        public class UniversityApiResponse
        {
            public string? name { get; set; }
            public List<string>? web_pages { get; set; }
            public List<string>? domains { get; set; }
            public string? alpha_two_code { get; set; }
            public string? state_province { get; set; }
        }
    }
}