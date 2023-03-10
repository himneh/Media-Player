using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Media_Player.Utils
{
    public class Preferences
    {
        private string _name;
        private string _path;

        private Dictionary<string, string> _dict;

        public Preferences(string name)
        {
            // create app data directory if not exists
            string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaPlayer");
            Directory.CreateDirectory(appDataDir);

            _name = name + ".json";
            _path = Path.Combine(appDataDir, _name);
            Debug.WriteLine("Create new Preferences: " + _path);

            // create file if not exists
            using (StreamWriter w = File.AppendText(_path)) { }

            using (StreamReader reader = new StreamReader(_path))
            {
                string json = reader.ReadToEnd();
                if (json != null && json.Length > 0)
                    _dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                else
                    _dict = new Dictionary<string, string>();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void put(string key, object value)
        {
            string json = JsonConvert.SerializeObject(value);
            putString(key, json);
        }

        public T? get<T>(string key)
        {
            string? valueStr = getString(key);
            if (valueStr == null) return default;
            try
            {
                return JsonConvert.DeserializeObject<T>(valueStr);
            }
            catch (Exception)
            {
                return default;
            }
        }

        public void remove(string key)
        {
            removeString(key);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void putString(string key, string value)
        {
            _dict[key] = value;
            string newJson = JsonConvert.SerializeObject(_dict, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(_path, false))
            {
                writer.WriteLine(newJson);
            }
        }

        private string? getString(string key) => _dict.ContainsKey(key) ? _dict[key] : null;

        private void removeString(string key)
        {
            _dict.Remove(key);
            string newJson = JsonConvert.SerializeObject(_dict, Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(_path, false))
            {
                writer.WriteLine(newJson);
            }
        }
    }
}
