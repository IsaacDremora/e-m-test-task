using System.Reflection;
using System.Runtime.CompilerServices;
using System.Validation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppContext>();
builder.Services.AddSwaggerGen();
builder.Services.AddFlatValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

/* набор параметров для метода .WriteTo.PostgreSQL()
   */

string tableName = "logs";
string ConnectionString = "Host=localhost;Database=OrdersAndDisctricts;Username=postgres;Password=1563;";
IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
{
    {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
    {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
    {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
    {"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
    {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
    {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
    {"props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
    {"machine_name", new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l") }
};

/* Настройка логгирования
   */

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.PostgreSQL(ConnectionString, tableName, columnWriters, needAutoCreateTable:true)
    .CreateLogger();

builder.Logging.AddSerilog();

var app = builder.Build();

app.UseForwardedHeaders();

if (builder.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI((o) => 
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        o.RoutePrefix = string.Empty;
    });
}

app.MapGet("/", (ILogger<Program> logger) => {
    logger.LogInformation("Запрос с использованием Serilog");
    return "hi";
});

app.MapGet("/orders", async (AppContext db) => await db.Orders.ToListAsync());

app.MapGet("/orders/{id}", async (AppContext db, int id) =>
{
    var order = await db.Orders.FirstAsync(d => d.OrderId == id);
    return order is not null? Results.Ok(order) : Results.NotFound();
});

app.MapGet("/districts", async (AppContext db) => await db.Districts.ToListAsync());

app.MapGet("/get-ip", (HttpContext httpContext) =>
{
    var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
    return Results.Ok(new { IpAddress = ipAddress });
});

/* метод для создания строк в таблице заказов.
ТЗ поменялось, но я решил оставить поле IP адреса
   */

app.MapPost("/orders", async (AppContext db, Order order, HttpContext context, ILogger<Program> logger) => 

{
    order.Ip = context.Connection.RemoteIpAddress?.ToString();
    order.OrderTime = DateTime.Now.ToUniversalTime();
    order.ExpectedDeliveryTime = DateTime.Now.AddMinutes(50f).ToUniversalTime(); // фиксированная доставка за 50 минут
    await db.Orders.AddAsync(order);
    await db.SaveChangesAsync();
    logger.LogInformation("order {order}", order);
    return Results.Created($"orders/{order.OrderId}", order);
});

app.MapPost("/districts", async (AppContext db, District district) =>
{
    await db.Districts.AddAsync(district);
    await db.SaveChangesAsync();
    return Results.Created($"districts/{district.DistrictId}", district);
});

app.MapPut("/districts", async (AppContext db,District update, int id) =>
{
    var dist = await db.Districts.FindAsync(id);
    if (dist == null) return Results.NotFound();
    dist.DistrictId = update.DistrictId;
    dist.DistrictName = update.DistrictName;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

/*
post запрос для того, чтобы завершить доставку заказа и узнать время доставки. 
*/

app.MapPost("/delivered-order", async (AppContext db, int id, Order order)=>{
    var delivered = await db.Orders.FindAsync(id);
    if (delivered == null) return Results.NotFound();
    delivered.DeliveryTime = DateTime.Now.ToUniversalTime();
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/orders/{id}", async (AppContext db, int id) =>
{
    var removeorder = await db.Orders.FindAsync(id);
    if (removeorder == null) return Results.NotFound();
    db.Orders.Remove(removeorder);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/districts/{id}", async (AppContext db, int id) => 
{
    var removedist = await db.Districts.FindAsync(id);
    if (removedist == null) return Results.NotFound();
    db.Districts.Remove(removedist);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

/* метод для фильтрации по IP адресу и внешнему ключу района города 
    (старый вариант ТЗ. Для корректной работы сваггер нужно вызывать не из одноранговой сети. Иначе будет всегда возвращаться нулевой адрес)
   */

app.MapGet("/deliver-Order/{ip}", async (AppContext db, int distId, string ip, ILogger<Program> logger) => 
{
    var orderIp = await db.Orders.FirstOrDefaultAsync(d => d.Ip == ip);
    var distOrder = await db.Orders.FindAsync(distId);
    if (orderIp == null) return Results.NotFound();
    if (distOrder == null) return Results.NotFound();
    var orderedOrder = await db.Orders
    .Where(x => x.DistrictId == distId && x.Ip == ip)
    .ToListAsync();
    logger.LogInformation("order {orderedOrder}", orderedOrder);    
    return Results.Ok(orderedOrder);
});

/* Метод для фильтрации заказов по внешнему ключу района города и первой дате заказа
   */

app.MapGet("/delivery-Order/{districtId}", async (AppContext db, int distId, DateTime firstDeliveryDateTime, ILogger<Program> logger) => 
{
    var minDateTimeOrder = await db.Orders
    .Where(d => d.DistrictId == distId && d.OrderTime == firstDeliveryDateTime)
    .OrderBy(d => d.OrderTime)
    .FirstOrDefaultAsync();
    if (minDateTimeOrder == null) return Results.NotFound();
    logger.LogInformation("first order time {minDateTimeOrder}", minDateTimeOrder.OrderTime);
    return Results.Ok(minDateTimeOrder);
});

app.Run();