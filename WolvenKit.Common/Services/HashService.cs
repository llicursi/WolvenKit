using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using WolvenKit.Common.FNV1A;
using WolvenKit.Common.Model;
using WolvenKit.Core.Compression;
using WolvenKit.Core.Exceptions;
using WolvenKit.Core.Extensions;
using WolvenKit.RED4.Types;
using WolvenKit.RED4.Types.Pools;

namespace WolvenKit.Common.Services
{
    public class HashService : IHashService
    {
        #region Fields

        private const string s_used = "WolvenKit.Common.Resources.usedhashes.kark";
        private const string s_unused = "WolvenKit.Common.Resources.unusedhashes.kark";
        private const string s_noderefs = "WolvenKit.Common.Resources.noderefs.kark";
        private const string s_userHashes = "user_hashes.txt";
        private const string s_missing = "WolvenKit.Common.Resources.missinghashes.json";

        private readonly Dictionary<ulong, SAsciiString> _hashes = new();
        private readonly Dictionary<ulong, SAsciiString> _additionalhashes = new();
        private readonly Dictionary<ulong, SAsciiString> _userHashes = new();
        private readonly Dictionary<ulong, SAsciiString> _projectHashes = new();
        private readonly Dictionary<ulong, SAsciiString> _noderefs = new();

        private Dictionary<ulong, string> _missing = new();

        #endregion Fields

        #region Constructors

        public HashService()
        {
            Load();

            ImportHandler.AddPathHandler = AddProjectPath;
            CNamePool.ResolveHashHandler = Get;

            NodeRefPool.ResolveHashHandler = GetNodeRef;
        }

        #endregion Constructors

        #region Methods

        private bool IsAdditionalLoaded;/*() => _additionalhashes.Count > 0;*/

        public IEnumerable<ulong> GetAllHashes()
        {
            // load additional
            LoadAdditional();
            return _hashes.Keys.Concat(_userHashes.Keys).Concat(_additionalhashes.Keys);
        }

        public IEnumerable<ulong> GetMissingHashes() => _missing.Keys;

        public bool Contains(ulong key)
        {
            if (_hashes.ContainsKey(key))
            {
                return true;
            }
            if (_userHashes.ContainsKey(key))
            {
                return true;
            }
            if (_missing.ContainsKey(key))
            {
                return false;
            }


            // load additional
            LoadAdditional();
            if (_additionalhashes.ContainsKey(key))
            {
                return true;
            }

            return false;
        }

        public string GetGuessedExtension(ulong key)
        {
            if (_missing.TryGetValue(key, out var ext))
            {
                return ext;
            }
            return null;
        }

        public string Get(ulong key)
        {
            if (_hashes.ContainsKey(key))
            {
                return _hashes[key].ToString();
            }

            if (_userHashes.ContainsKey(key))
            {
                return _userHashes[key].ToString();
            }

            if (_projectHashes.ContainsKey(key))
            {
                return _projectHashes[key].ToString();
            }

            // load additional
            LoadAdditional();
            if (_additionalhashes.ContainsKey(key))
            {
                return _additionalhashes[key].ToString();
            }

            return null;
        }

        public string GetNodeRef(ulong key)
        {
            if (_noderefs.ContainsKey(key))
            {
                return _noderefs[key].ToString();
            }

            return Get(key);
        }

        public void AddCustom(ulong hash, string path)
        {
            if (!Contains(hash))
            {
                _userHashes.Add(hash, new SAsciiString(path));
            }
        }

        public void AddCustom(string path)
        {
            var hash = FNV1A64HashAlgorithm.HashString(path);
            if (!Contains(hash))
            {
                _userHashes.Add(hash, new SAsciiString(path));
            }
        }

        public void ClearProjectHashes() => _projectHashes.Clear();

        public void AddProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var hash = FNV1A64HashAlgorithm.HashString(path);

            if (_hashes.ContainsKey(hash))
            {
                return;
            }

            if (_userHashes.ContainsKey(hash))
            {
                return;
            }

            if (_projectHashes.ContainsKey(hash))
            {
                return;
            }

            _projectHashes.Add(hash, new SAsciiString(path));
        }

        public List<string> GetProjectHashes() => _projectHashes.Select(pair => pair.Value.ToString()).ToList();


        private void LoadAdditional()
        {
            if (IsAdditionalLoaded)
            {
                return;
            }

            //LoadEmbeddedHashes(s_unused, _additionalhashes);
            IsAdditionalLoaded = true;
        }

        private void Load()
        {
            LoadEmbeddedHashes(s_used, _hashes);
            LoadEmbeddedHashes(s_noderefs, _noderefs);

            LoadAdditional();

            // user hashes
            LoadUserHashesFrom(Path.GetDirectoryName(AppContext.BaseDirectory));
            LoadUserHashesFrom(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "REDModding", "WolvenKit"));

            LoadMissingHashes();
        }

        private void LoadUserHashesFrom(string path)
        {
            var userHashesPath = Path.Combine(path ?? throw new InvalidOperationException(), s_userHashes);
            if (File.Exists(userHashesPath))
            {
                using var userFs = new FileStream(userHashesPath, FileMode.Open, FileAccess.Read);
                ReadHashes(userFs, _userHashes);
            }
        }

        private void LoadEmbeddedHashes(string resourceName, Dictionary<ulong, SAsciiString> hashDictionary)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            // read KARK header
            var oodleCompression = stream.ReadStruct<uint>();
            if (oodleCompression != Oodle.KARK)
            {
                throw new DecompressionException($"Incorrect hash file.");
            }

            var outputsize = stream.ReadStruct<uint>();

            // read the rest of the stream
            var outputbuffer = new byte[outputsize];

            var inbuffer = stream.ToByteArray(true);

            Oodle.Decompress(inbuffer, outputbuffer);

            hashDictionary.EnsureCapacity(1_100_000);

            using var ms = new MemoryStream(outputbuffer);
            ReadHashes(ms, hashDictionary);

        }

        private void LoadMissingHashes()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_missing);
            if (stream == null)
            {
                throw new FileNotFoundException(s_missing);
            }
            _missing = JsonSerializer.Deserialize<Dictionary<ulong, string>>(stream);
        }

        private void ReadHashes(Stream memoryStream, IDictionary<ulong, SAsciiString> hashDict)
        {
            using var sr = new StreamReader(memoryStream);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var hash = FNV1A64HashAlgorithm.HashString(line);
                if (_hashes.ContainsKey(hash))
                {
                    continue;
                }
                if (_additionalhashes.ContainsKey(hash))
                {
                    continue;
                }
                if (_userHashes.ContainsKey(hash))
                {
                    continue;
                }

                if (!hashDict.ContainsKey(hash))
                {
                    hashDict.Add(hash, new SAsciiString(line));
                }
            }
        }

        #endregion Methods
    }
}
