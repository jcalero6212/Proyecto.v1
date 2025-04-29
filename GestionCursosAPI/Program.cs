
var builder = WebApplication.CreateBuilder(args);

// HABILITAR CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
               // tu frontend
               .AllowAnyHeader()
              .AllowAnyMethod();
    });
});




// Agrega los controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuración del entorno
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usa HTTPS
app.UseHttpsRedirection();

// AQUI USAMOS CORS ANTES DE AUTH
app.UseCors("PermitirFrontend");

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "login.html" }
});
app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();



