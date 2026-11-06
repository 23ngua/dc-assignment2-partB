using DataWebService.Database;
using DataWebService.Models.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(e => e.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>(); // register instance of password hasher
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string dbFile = builder.Configuration["Database:FileName"] ?? "mydatabase.db";
DBConnection.Configure(Path.Combine(builder.Environment.ContentRootPath, dbFile));
DBManager.DBInitialize();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();