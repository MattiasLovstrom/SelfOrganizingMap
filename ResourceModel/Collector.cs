using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ResourceModel
{
    public class Collector
    {
        private readonly Dictionary<string, List<string>> _lists = new Dictionary<string, List<string>>();
        private readonly List<RawDocument> _data = new List<RawDocument>();
        private readonly string _dataFilePath;
        public List<string> Keys { get; }

        public Collector(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                _lists.Add(key, new List<string>());
            }
        }

        public Collector(string path)
        {
            var serializer = new JsonSerializer();
            using var keysFile = File.OpenText($"{path}\\keys.json");
            Keys = (List<string>)serializer.Deserialize(keysFile, typeof(List<string>));
            foreach (var key in Keys)
            {
                var fileName = $"{path}\\{key}.json";
                if (File.Exists(fileName))
                {
                    using var listFile = File.OpenText(fileName);
                    _lists.Add(key, (List<string>)serializer.Deserialize(listFile, typeof(List<string>)));
                }
                else
                {
                    _lists.Add(key, new List<string>());
                }
            }

            _dataFilePath = $"{path}\\data.json";
            using var dataFile = File.OpenText(_dataFilePath);
            var data = (RawSearchResult)serializer.Deserialize(dataFile, typeof(RawSearchResult));
            foreach (var docList in data.Components.DocLists)
            {
                foreach (var document in docList.Documents)
                {
                    Import(document);
                }
            }
        }

        public void Import(RawDocument document)
        {
            _data.Add(document);
            foreach (var key in _lists.Keys)
            {
                if (!document.TryGetValue(key, out var rawValue)) continue;
                var value = rawValue as string;
                if (value == null || _lists[key].Contains(value)) continue;

                _lists[key].Add(value);
            }
        }

        public IEnumerable<RawDocument> Documents => _data;

        public double[,] _inputData;
        public double[,] GetInputVectors()
        {
            if (_inputData != null) return _inputData;
            _inputData = new double[_data.Count, _lists.Count];
            for (var row = 0; row < _data.Count; row++)
            {
                for (var column = 0; column < Keys.Count; column++)
                {
                    if (_data[row].TryGetValue(Keys[column], out var value))
                    {
                        var list = _lists[Keys[column]];
                        _inputData[row, column] =
                            1.0 * (list.IndexOf((string)value) + 1.0) / (list.Count + 1);
                    }
                }
            }

            return _inputData;
        }


        public double[] ToNormalizedData(RawDocument data)
        {
            var inputData = new double[Keys.Count];

            for (var column = 0; column < Keys.Count; column++)
            {
                if (data.TryGetValue(Keys[column], out var value))
                {
                    var list = _lists[Keys[column]];
                    inputData[column] =
                        1.0 * (list.IndexOf((string)value) + 1.0) / (list.Count + 1);
                }
            }

            return inputData;
        }

        public IEnumerable<string> GetList(string key)
        {
            return _lists[key];
        }

        public void SetList(string key, IEnumerable<string> list)
        {
            _lists[key] = new List<string>(list);
        }

        public void Save(string resultFolder)
        {
            if (!Directory.Exists(resultFolder))
            {
                Directory.CreateDirectory(resultFolder);
            }
            File.Copy(_dataFilePath, $"{resultFolder}\\data.json");
            File.WriteAllText($"{resultFolder}\\keys.json", JsonConvert.SerializeObject(Keys, Formatting.Indented));

            foreach (var key in Keys)
            {
                var fileName = $"{resultFolder}\\{key}.json";
                File.WriteAllText(fileName, JsonConvert.SerializeObject(_lists[key], Formatting.Indented));
            }
        }
    }
}