using Microsoft.EntityFrameworkCore;
using TodoMinimalApi;

class TodoDbOld : DbContext
{
    public TodoDbOld(DbContextOptions<TodoDb> options) : base(options) { }
    public DbSet<Todo> Todos => Set<Todo>();
}