using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Escape.Persistence;

class Game
{
    [Key]
    [MaxLength(32)]
    public string Name { get; set; }
    public DateTime Time { get; set; }

    public int TableMins { get; set; }
    public int TableSecs { get; set; }

    public int TableSize { get; set; }

    public ICollection<Field> Fields { get; set; }

    public Game()
    {
        Fields = new List<Field>();
        Time = DateTime.Now;
    }
}
