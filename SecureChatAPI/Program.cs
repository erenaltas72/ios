using Microsoft.EntityFrameworkCore;
using SecureChatAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- VERİTABANI ---
builder.Services.AddDbContext<ChatContext>(options =>
    options.UseSqlite("Data Source=chat.db"));

// --- YENİ EKLENDİ (1. Parça) ---
// Her yerden gelen isteklere izin ver (Ödev olduğu için güvenlik sınırlarını kaldırıyoruz)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()   // Herkese izin ver (Web, Mobil vs.)
                   .AllowAnyMethod()   // GET, POST, PUT hepsine izin ver
                   .AllowAnyHeader();  // Tüm başlıklara izin ver
        });
});
// -------------------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- YENİ EKLENDİ (2. Parça) ---
// Burası çok önemli: HttpsRedirection'dan sonra, Authorization'dan önce olsun.
app.UseCors("AllowAll"); 
// -------------------------------

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();