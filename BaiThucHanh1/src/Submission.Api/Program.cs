using System.Net.Http.Json;

var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5000";
using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

Console.WriteLine("--- DANG LAY DANH SACH BAI TAP ---");
var assignments = await client.GetFromJsonAsync<List<Assignment>>("/api/assignments");
if (assignments != null)
{
    foreach (var item in assignments)
    {
        Console.WriteLine($" - {item.Id}: {item.Title} - {item.Description}");
    }
}

Console.WriteLine("\n--- DANG GUI BAI NOP ---");
var request = new CreateSubmissionRequest(1, "SV001", "[https://github.com/example/repo](https://github.com/example/repo)");
var response = await client.PostAsJsonAsync("/api/submissions", request);

response.EnsureSuccessStatusCode();

var result = await response.Content.ReadFromJsonAsync<SubmissionResult>();
if (result != null)
{
    Console.WriteLine($"Da nop bai thanh cong!");
    Console.WriteLine($"Ma luot nop: {result.Id}");
    Console.WriteLine($"Trang thai: {result.Status}");
}

record Assignment(int Id, string Title, string Description);
record CreateSubmissionRequest(int AssignmentId, string StudentCode, string RepositoryUrl);
record SubmissionResult(Guid Id, int AssignmentId, string StudentCode, string Status, DateTimeOffset ReceivedAt);
