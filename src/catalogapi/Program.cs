using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ Singleton
builder.Services.AddSingleton<GreetingService>();

var app = builder.Build();

// --- 1. DỮ LIỆU MẪU SẢN PHẨM (Dữ liệu tạm trong RAM) ---
var products = new List<ProductDto>
{
    new(1, "Giao trinh Lap trinh phan tan", "Giao trinh chinh thuc", 150000),
    new(2, "Tai lieu RESTful API", "Huong dan thiet ke API", 50000),
    new(3, "Bai tap thuc hanh .NET", "Bo bai tap kem loi giai", 120000)
};

// --- 2. CÁC ENDPOINT ---

// Endpoint GET gốc
app.MapGet("/", () => Results.Ok(new {
    App = "catalogapi",
    Message = "ASP.NET Core Minimal API (catalogapi) is running successfully!"
}));

// Endpoint GET danh sách sản phẩm
app.MapGet("/api/products", () => Results.Ok(products));

// GET /api/products/{id} : chi tiết một sản phẩm
app.MapGet("/api/products/{id:int}", (int id) => {
    var product = products.FirstOrDefault(p => p.Id == id);
    if (product is null)
    {
        return Results.NotFound(new ApiError("NOT_FOUND", "Khong tim thay san pham."));
    }
    return Results.Ok(product);
});

// POST /api/products : nhận body JSON, kiểm tra hợp lệ và tạo mới sản phẩm[cite: 1]
app.MapPost("/api/products", (CreateProductRequest request) => {
    // Kiểm tra tính hợp lệ dữ liệu ngay ở cửa vào[cite: 1]
    if (string.IsNullOrWhiteSpace(request.Name) || request.Price <= 0) {
        return Results.BadRequest(new ApiError(
            "INVALID_PRODUCT",
            "Tên sản phẩm không được để trống và giá phải lớn hơn 0."
        ));
    }

    // Tạo sản phẩm mới và thêm vào danh sách tạm
    var newProduct = new ProductDto(
        Id: products.Count > 0 ? products.Max(p => p.Id) + 1 : 1,
        Name: request.Name.Trim(),
        Description: "Sản phẩm tạo mới từ API",
        Price: request.Price
    );

    products.Add(newProduct);

    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
});

// Endpoint GET /hello/{name} kết hợp Route và Query
app.MapGet("/hello/{name}", (string name, string? language, GreetingService service) => {
    bool isEnglish = !string.IsNullOrEmpty(language) && language.Equals("en", StringComparison.OrdinalIgnoreCase);
    string message = isEnglish ? $"Hello, {name}!" : $"Xin chào, {name}!";
    
    return Results.Ok(new {
        Greeting = message,
        Timestamp = DateTime.UtcNow
    });
});

// Endpoint POST /greetings nhận JSON và kiểm tra dữ liệu
app.MapPost("/greetings", (CreateGreetingRequest request, GreetingService service) => {
    if (string.IsNullOrWhiteSpace(request.Name)) {
        return Results.BadRequest(new ApiError("INVALID_NAME", "Tên không được để trống hoặc chỉ chứa khoảng trắng."));
    }

    string cleanName = request.Name.Trim();
    var storedGreeting = service.Create(cleanName);

    return Results.Created($"/greetings/{storedGreeting.Id}", storedGreeting);
});

app.Run();


// --- 3. DTO VÀ CÁC LỚP DỊCH VỤ ---

record ProductDto(int Id, string Name, string Description, decimal Price);
record CreateProductRequest(string Name, decimal Price); // Dữ liệu POST truyền vào[cite: 1]
record ApiError(string Code, string Message); // Cấu trúc trả về lỗi chuẩn[cite: 1]

public class GreetingService
{
    private int _counter = 0;

    public StoredGreeting Create(string name)
    {
        int id = Interlocked.Increment(ref _counter);
        return new StoredGreeting
        {
            Id = id,
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}

public record CreateGreetingRequest(string Name);

public record StoredGreeting
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}