using SqlDetective.Domain.Progress.Data;
using SqlDetective.Domain.Progress.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlDetective.Data.InMemory
{
    public class InMemoryGameProgressRepository : IGameProgressRepository
    {
        private readonly ConcurrentDictionary<string, GameProgress> r_GamesDict = new ConcurrentDictionary<string, GameProgress>();
        
        public Task<GameProgress?> LoadAsync(string key, CancellationToken cancellationToken = default)
        {
            r_GamesDict.TryGetValue(key, out GameProgress? o_GameProgress);
            
            return Task.FromResult(o_GameProgress);
        }

        public Task SaveAsync(GameProgress progress, CancellationToken cancellationToken = default)
        {
            r_GamesDict[progress.Key] = progress;
            return Task.CompletedTask;
        }
    }
}
