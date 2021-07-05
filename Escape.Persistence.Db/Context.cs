using System;
using System.Data.Entity;

class Context : DbContext
{
    public Context(String connection)
        : base(connection)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<Field> Fields { get; set; }
}