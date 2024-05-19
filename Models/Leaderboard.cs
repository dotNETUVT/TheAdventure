using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public class Leaderboard
{
    private List<(string PlayerName, int Score)> _scores = new List<(string PlayerName, int Score)>();
    private int _maxEntries = 10; // Maximum number of entries to display on the leaderboard

    public void AddScore(string playerName, int score)
    {
        _scores.Add((playerName, score));
        _scores.Sort((a, b) => b.Score.CompareTo(a.Score)); // Sort descending
        TrimExcessEntries();
    }

    public void Display()
    {
        Console.WriteLine("Leaderboard:");
        int rank = 1;
        foreach (var (playerName, score) in _scores.Take(_maxEntries))
        {
            Console.WriteLine($"{rank++}. {playerName}: {score}");
        }
    }

    public void Clear()
    {
        _scores.Clear();
    }

    public void SaveToFile(string filePath)
    {
        var jsonString = JsonSerializer.Serialize(_scores);
        File.WriteAllText(filePath, jsonString);
    }

    public void LoadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            _scores = JsonSerializer.Deserialize<List<(string, int)>>(jsonString);
        }
        else
        {
            Console.WriteLine("Leaderboard file not found.");
        }
    }

    private void TrimExcessEntries()
    {
        if (_scores.Count > _maxEntries)
        {
            _scores = _scores.Take(_maxEntries).ToList();
        }
    }
}
