using Npgsql;
using WebApplication;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(policy => policy.AddPolicy("default", opt =>
{
    opt.AllowAnyHeader();
    opt.AllowCredentials();
    opt.AllowAnyMethod();
    opt.SetIsOriginAllowed(_ => true);
}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseCors("default");
}

app.UseHttpsRedirection();

app.Map("/", () => Results.LocalRedirect("/data"));
app.MapGet("/data", (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT title, area FROM data;";

    var data = new List<Data>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
        data.Add(new(reader.GetString(0), reader.GetInt32(1)));

    return Results.Ok(data);
});

app.MapGet("/mean_area", (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = "SELECT area FROM data;";

    var areas = new List<double>();
    using var reader = command.ExecuteReader();
    while (reader.Read())
        areas.Add(reader.GetInt32(0));

    return Results.Ok(Utils.ComputeMeanArea(areas));
});

Prepare(app.Services.GetService<IConfiguration>() ?? throw new NullReferenceException("Configuration Service is null"));

app.Run();

static void Prepare(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("Database");
    using var connection = new NpgsqlConnection(connectionString);

    connection.Open();
    using var transation = connection.BeginTransaction();

    using var dropCommand = new NpgsqlCommand("DROP TABLE IF EXISTS data;", connection, transation);
    dropCommand.ExecuteNonQuery();

    using var createTableCommand = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS data (id serial NOT NULL, title varchar(255), area int, PRIMARY KEY (id));", connection, transation);
    createTableCommand.ExecuteNonQuery();

    for (var i = 0; i < 5; i++)
    {
        using var insertCommand = new NpgsqlCommand($"INSERT INTO data (title, area) VALUES ('Data #{i}', {i * 10});", connection, transation);
        insertCommand.CommandText = "INSERT INTO data (title, area) VALUES (@Name, @Area);";
        insertCommand.Parameters.AddWithValue("@Name", $"Data #{i}");
        insertCommand.Parameters.AddWithValue("@Area", i * 10);
        insertCommand.ExecuteNonQuery();
    }
    transation.Commit();
    connection.Close();
}

record Data(string Title, int Area);
