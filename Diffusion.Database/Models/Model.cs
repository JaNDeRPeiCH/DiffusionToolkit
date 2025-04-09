﻿using SQLite;

namespace Diffusion.Database.Models;

public class Model
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public string Hash { get; set; }
    public string Hashv2 { get; set; }
    public string SHA256 { get; set; }
}
