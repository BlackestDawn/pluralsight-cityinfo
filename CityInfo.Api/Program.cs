using System;
using CityInfo.Api;
using CityInfo.Api.DbContexts;
using CityInfo.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddXmlDataContractSerializerFormatters();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = ctx =>
{
  ctx.ProblemDetails.Extensions.Add("server", Environment.MachineName);
});

#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddDbContext<CityInfoContext>(dbContextOptions =>
  dbContextOptions.UseSqlite(
    builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]
    ?? throw new InvalidOperationException()
  ));
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
builder.Services.AddScoped<IPointofInterestService, PointOfInterestService>();

builder.Services.AddAutoMapper(config => { },
  AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
