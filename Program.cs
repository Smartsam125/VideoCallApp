using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;  
using Serilog.Events;
using System.Text;
using VideoCallApp.Data;
using VideoCallApp.Hubs;
using VideoCallApp.Interfaces;
using VideoCallApp.Services;

namespace VideoCallApp
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting Video Call App...");

                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog((ctx, lc) => lc
          .MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
          .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
          .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Error)
          .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
          .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
          .Enrich.FromLogContext().WriteTo.File("./logs/Video-Call-App-.txt",
           rollingInterval: RollingInterval.Day, retainedFileCountLimit: 186)
          .WriteTo.Console());

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

              
                var jwtSettings = builder.Configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

                builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
                builder.Services.AddHttpLogging(logging =>
                {
                    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
                });


                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IChatService, ChatService>();
                builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
                builder.Services.AddSignalR(options =>
                {
                    options.EnableDetailedErrors = true;
                    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                });

                
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                var app = builder.Build();

                app.UseHttpLogging();
                app.UseSerilogRequestLogging();
                //if (app.Environment.IsDevelopment())
                //{
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
               // }
                //else
                //{
                //    app.UseExceptionHandler("/Error");
                  //  app.UseHsts();
               // }

                app.UseHttpsRedirection();
                app.UseCors();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();
                app.MapHub<ChatHub>("/chatHub");

                app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    try
                    {
                        var context = services.GetRequiredService<ApplicationDbContext>();
                        context.Database.EnsureCreated();

                        Log.Information("Database initialized successfully. User count: {UserCount}", context.Users.Count());
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "An error occurred while initializing the database");
                        throw;
                    }
                }

                Log.Information("Video Call App started successfully at {Time}", DateTimeOffset.Now);

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
