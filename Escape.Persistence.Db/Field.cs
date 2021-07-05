using System;
using System.ComponentModel.DataAnnotations;
using Escape.Persistence;


class Field
{
    [Key]
    public int Id { get; set; }

    public int X { get; set; }
    
    public int Y { get; set; }
    
    public int Value { get; set; }
    
    public Game Game { get; set; }
}
