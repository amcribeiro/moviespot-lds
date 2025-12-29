using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieSpot.Data;
using MovieSpot.Services.Background;
using MovieSpot.Services.Bookings;
using MovieSpot.Services.CinemaHalls;
using MovieSpot.Services.Cinemas;
using MovieSpot.Services.Emails;
using MovieSpot.Services.Genres;
using MovieSpot.Services.Handlers;
using MovieSpot.Services.Invoices;
using MovieSpot.Services.Movies;
using MovieSpot.Services.Notifications;
using MovieSpot.Services.Payments;
using MovieSpot.Services.Reviews;
using MovieSpot.Services.Seats;
using MovieSpot.Services.Sessions;
using MovieSpot.Services.Tmdb;
using MovieSpot.Services.Stats;
using MovieSpot.Services.Tokens;
using MovieSpot.Services.Users;
using MovieSpot.Services.Vouchers;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

var env = builder.Environment.EnvironmentName;

if (env == "Test")
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("MovieSpot_TestDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
}

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (env != "Test")
{
    builder.Services.AddHostedService<GenreSyncService>();
    builder.Services.AddHostedService<TrendingMoviesSyncService>();
    builder.Services.AddHostedService<BookingReminderService>();
    builder.Services.AddHostedService<BookingExpirationBackgroundService>();
}

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<ICinemaHallService, CinemaHallService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ITMDBAPIService, TmdbApiService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ISeatService, SeatService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();

builder.Services.Configure<FcmOptions>(builder.Configuration.GetSection(FcmOptions.SectionName));
builder.Services.AddSingleton<IFcmNotificationService, FcmNotificationService>();


builder.Services.AddHttpClient<ITMDBAPIService, TmdbApiService>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["TMDB:BaseUrl"];
    if (!baseUrl.EndsWith("/")) baseUrl += "/";
    http.BaseAddress = new Uri(baseUrl);
    http.DefaultRequestHeaders.Accept.ParseAdd("application/json");

    var token = cfg["TMDB:ApiKey"];
    http.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
});
var jwtKey = builder.Configuration["JwtConfig:Key"];
var jwtIssuer = builder.Configuration["JwtConfig:Issuer"];

builder.Services.AddHttpClient();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
}


app.Run();
public partial class Program { }

