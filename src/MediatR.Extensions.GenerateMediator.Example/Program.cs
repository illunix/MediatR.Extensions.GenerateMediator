using MediatR;
using MediatR.Extensions.GenerateMediator.Example;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMediatR(typeof(Program));

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
