using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // This configures to call Identity service to verify the token everytime
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["IdentityServiceUrl"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.NameClaimType = "username";
        });
}

var app = builder.Build();

{
    app.MapReverseProxy();
    app.UseAuthentication();
    app.UseAuthorization();
}

app.Run();
