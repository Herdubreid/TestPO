using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Blazor.Extensions.Storage.Interfaces;

namespace TestPO
{
    class FileStorage : ILocalStorage
    {
        public ValueTask Clear()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<TItem> GetItem<TItem>(string key)
        {
            try
            {
                var fs = File.OpenText($"{key}.json");
                var json = await fs.ReadToEndAsync();
                return JsonSerializer.Deserialize<TItem>(json);
            }
            catch
            {
                return default;
            }
        }

        public ValueTask<string> Key(int index)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> Length()
        {
            throw new NotImplementedException();
        }

        public ValueTask RemoveItem(string key)
        {
            throw new NotImplementedException();
        }

        public async ValueTask SetItem<TItem>(string key, TItem item)
        {
            try
            {
                var json = JsonSerializer.Serialize(item);
                var fs = File.CreateText($"{key}.json");
                await fs.WriteAsync(json);
                fs.Close();
            }
            catch { }
        }
    }
}
