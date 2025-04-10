﻿using SQLite;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Size = System.Drawing.Size;

namespace Diffusion.Toolkit.Thumbnails;

public class CacheEntry
{
    public DateTime Created { get; set; }
    public BitmapSource BitmapSource { get; set; }
}

public class ThumbnailCacheOld
{
    private readonly int _maxItems;
    private readonly int _evictItems;
    private ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

    private object _lock = new object();
    private static ThumbnailCacheOld _instance;

    public static ThumbnailCacheOld Instance => _instance;

    private ThumbnailCacheOld(int maxItems, int evictItems)
    {
        _maxItems = maxItems;
        _evictItems = evictItems;
    }

    public bool TryGetThumbnail(string path, out BitmapSource? thumbnail)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(path, out var cacheEntry))
            {
                thumbnail = cacheEntry.BitmapSource;
                return true;
            }

            thumbnail = null;

            return false;
        }
    }

    public void AddThumbnail(string path, BitmapSource? thumbnail)
    {
        lock (_lock)
        {
            if (_cache.Count + 1 > _maxItems)
            {
                var evictions = _cache.OrderByDescending(c => c.Value.Created).Take(_evictItems);

                foreach (var eviction in evictions)
                {
                    _cache.TryRemove(eviction);
                }
            }
        }


        if (!_cache.TryAdd(path, new CacheEntry()
        {
            BitmapSource = thumbnail,
            Created = DateTime.Now
        }))
        {

        }
    }

    //public static void CreateInstance()
    //{
    //    _instance = new ThumbnailCache(500, 100);
    //}


    public static void CreateInstance(int maxItems, int evictItems)
    {
        _instance = new ThumbnailCacheOld(maxItems, evictItems);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}

public class Thumbnail
{
    public string Filename { get; set; }
    public byte[] Data { get; set; }
    public int Size { get; set; }
}


public class ThumbnailCache
{
    private static ThumbnailCache _instance;

    public static ThumbnailCache Instance => _instance;

    private readonly ConcurrentDictionary<string, SQLiteConnection> _connectionPool;

    private ThumbnailCache()
    {
        _connectionPool = new ConcurrentDictionary<string, SQLiteConnection>();
    }


    public SQLiteConnection OpenConnection(string path)
    {
        var dbPath = Path.GetDirectoryName(path);

        if (!_connectionPool.TryGetValue(dbPath, out var db))
        {
            db = new SQLiteConnection(Path.Combine(dbPath, "dt_thumbnails.db"));
            db.CreateTable<Thumbnail>();
            db.CreateIndex<Thumbnail>(t => t.Filename, true);
            _connectionPool[dbPath] = db;
        }

        return db;
    }


    public bool Unload(string path)
    {
        if (_connectionPool.TryRemove(path, out var db))
        {
            db.Close();
            return true;
        }

        return false;
    }

    public bool TryGetThumbnail(string path, int size, out BitmapSource? thumbnail)
    {
        var db = OpenConnection(path);

        var filename = Path.GetFileName(path);

        var data = db.Query<Thumbnail>("SELECT Filename, Data FROM Thumbnail WHERE Filename = ? AND Size = ?", filename, size);

        var result = false;

        if (data.Count > 0)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(data[0].Data);
            result = true;
            bitmap.EndInit();
            bitmap.Freeze();
            thumbnail = bitmap;
        }
        else
        {
            thumbnail = null;
        }

        return result;
    }

    public void AddThumbnail(string path, int size, BitmapImage bitmapImage)
    {
        if (File.Exists(path))
        {
            var db = OpenConnection(path);

            var filename = Path.GetFileName(path);
            var data = ((MemoryStream)bitmapImage.StreamSource).ToArray();

            var command = db.CreateCommand("REPLACE INTO Thumbnail (Filename, Data, Size) VALUES (@Filename, @Data, @Size)");
            command.Bind("@Filename", filename);
            command.Bind("@Data", data);
            command.Bind("@Size", size);
            command.ExecuteNonQuery();
        }
    }


    public static void CreateInstance(int maxItems, int evictItems)
    {
        _instance = new ThumbnailCache();
    }

    public void Clear()
    {
    }
}