using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TodoMinimalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.IncludeFields = true;
});
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

app.MapGet("/", () => Results.Json(new Todo { Name = "Walk dog", IsComplete = false }, options));

app.MapGet("/todoitems", async (TodoDb db) => await db.Todos.Select(t => new TodoItemDTO(t)).ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) => await db.Todos.Where(t => t.IsComplete).Select(t => new TodoItemDTO(t)).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) => await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(new TodoItemDTO(todo)) : Results.NotFound());

app.MapPost("/todoitems", async (TodoItemDTO todo, TodoDb db) =>
{
    var todoItem = new Todo
    {
        IsComplete = todo.IsComplete,
        Name = todo.Name,
    };
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
});

app.MapPut("/todoitems/{id}", async (int id, TodoItemDTO inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null) return Results.NotFound();
    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoItemDTO(todo));
    }
    return Results.NotFound();
});

app.Run();


public class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
    public string? Secret { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

public class TodoItemDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public TodoItemDTO() { }
    public TodoItemDTO(Todo todoItem) =>
    (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}