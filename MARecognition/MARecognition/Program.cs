using MARecognition.Services;
using MARecognition.Services.MultimodalActivityRecognition_CSD.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Increase upload limit
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000; // 500 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000; // 500 MB
});

// Services
builder.Services.AddSingleton<SemanticKernelService>();
builder.Services.AddSingleton<FrameExtractorService>();
builder.Services.AddSingleton<VideoAnalyzerService>(sp =>
    new VideoAnalyzerService("llava:latest"));
builder.Services.AddSingleton<EventLogManagerService>();

// New audio services
builder.Services.AddSingleton<AudioTranscriptionService>();
builder.Services.AddSingleton<AudioActivityRecoService>();
builder.Services.AddSingleton<AudioPeakDetectService>();


// multimodal fusion
builder.Services.AddSingleton<VideoAudioFusionService>(sp =>
    new VideoAudioFusionService(
        sp.GetRequiredService<FrameExtractorService>(),
        sp.GetRequiredService<VideoAnalyzerService>(),
        sp.GetRequiredService<AudioTranscriptionService>(),
        sp.GetRequiredService<AudioActivityRecoService>(),
        sp.GetRequiredService<EventLogManagerService>()
    ));

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
