using Microsoft.EntityFrameworkCore;
using UrlShort.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApiDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
// Add services to the container.


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
app.MapGet("/", ctx =>
{
	ctx.Response.ContentType = "text/html";
	return ctx.Response.SendFileAsync("index.html");
});




app.MapPost("/shorturl", async(UrlDto url, ApiDbContext db, HttpContext ctx) =>
{
	if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var inputUrl))
		return Results.BadRequest("Invalid Url");

	var random = new Random();
	const string chars = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
	var randomStr = new string(Enumerable.Repeat(chars, 4).Select(x => x[random.Next(x.Length)]).ToArray());

	var sUrl = new UrlManagement()
	{
		Url = url.Url,
		ShortUrl = randomStr
	};
	await db.Urls.AddAsync(sUrl);
	await db.SaveChangesAsync();
	var result = $"{ctx.Request.Scheme}://{ctx.Request.Host}/{sUrl.ShortUrl}";
	return Results.Ok(new UrlShortResponseDto()
	{
		Url = result
	});

});

app.MapFallback(async (ApiDbContext db, HttpContext ctx) =>
{
	var path = ctx.Request.Path.ToUriComponent().Trim('/');
	var urlMatch = await db.Urls.FirstOrDefaultAsync(x =>
					x.ShortUrl.Trim() == path.Trim());
	if (urlMatch == null)
		return Results.BadRequest("Invalid");
	return Results.Redirect(urlMatch.Url);

});

await app.RunAsync();

class ApiDbContext : DbContext
{
	public virtual DbSet<UrlManagement> Urls { get; set; }
    public ApiDbContext(DbContextOptions<ApiDbContext>options):base(options)
	{

	}

}