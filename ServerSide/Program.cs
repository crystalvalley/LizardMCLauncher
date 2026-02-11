using Microsoft.EntityFrameworkCore;
using ServerSide.Data;
using ServerSide.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SettingProvider>();
builder.Services.AddScoped<NoticeService>();
builder.Services.AddScoped<CloudRestService>();
builder.Services.AddScoped<VersionInfoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();