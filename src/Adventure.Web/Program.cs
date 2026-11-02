using Adventure.Web.Hubs;
using Adventure.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var signalR = builder.Services.AddSignalR();
if (!string.IsNullOrWhiteSpace(builder.Configuration["Azure:SignalR:ConnectionString"]))
{
    signalR.AddAzureSignalR();
}

builder.Services.AddSingleton<VoteStore>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/join/{sessionId}", (string sessionId) =>
    Results.Redirect($"/?session={Uri.EscapeDataString(sessionId)}"));

app.MapHub<AdventureHub>("/adventureHub");
app.Run();
