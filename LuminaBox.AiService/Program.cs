using LuminaBox.AiService.Chat;
using LuminaBox.AiService.Grpc;
using LuminaBox.AiService.Llm;
using LuminaBox.AiService.Options;
using LuminaBox.AiService.Tools;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));
builder.Services.Configure<AiServiceOptions>(builder.Configuration.GetSection(AiServiceOptions.SectionName));

builder.Services.AddHttpClient(OllamaChatClient.HttpClientName, (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
    // ngrok free tunnels show an HTML warning page to browser-like clients; this header skips it.
    client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
});

builder.Services.AddSingleton<OllamaChatClient>();
builder.Services.AddScoped<ChatOrchestrator>();
builder.Services.AddScoped<ForcedToolRouter>();
builder.Services.AddSingleton<StallDetector>();

// Tools the model may call — add new ones here.
builder.Services.AddScoped<IAiTool, GetCurrentTimeTool>();
builder.Services.AddScoped<IAiTool, GuideUserTool>();

builder.Services.AddSingleton<ApiKeyInterceptor>();
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ApiKeyInterceptor>();
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
builder.Services.AddGrpcReflection();

var app = builder.Build();

app.MapGrpcService<AiChatGrpcService>();

if (app.Environment.IsDevelopment())
    app.MapGrpcReflectionService(); // lets grpcurl / Postman discover the service

app.Run();
