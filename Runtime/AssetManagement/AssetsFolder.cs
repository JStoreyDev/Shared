using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace JS
{
    [Serializable]
    public class AssetsFolder : IEnumerable<AssetInfo>
    {
        static AssetsFolder _instance;
        static AssetsFolder Instance => _instance ?? (_instance = new AssetsFolder());
        bool _loaded;
        public static void Load() => Instance.LoadAssets();

        
        #if UNITY_EDITOR

        public static void AddToProject(AssetInfo info)
        {
            var assetPath = Path.Combine(AssetStore5, info.Publisher, info.Category, info.FileName);
            UnityEditor.AssetDatabase.ImportPackage(assetPath,false);
            UnityEditor.AssetDatabase.Refresh();
        }
        
        #endif
        
        
        public static void ForceRefresh() => Instance.LoadAssets(true);
        
        public static List<AssetInfo> Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return new List<AssetInfo>();
            if(!Instance._loaded) Instance.LoadAssets();
            return Instance.SearchAll(search);
        }
        
        [SerializeField] List<AssetInfo> _cachedAssets = new List<AssetInfo>();

        DateTime _lastLoaded;
        [JsonIgnore]
        Autocomplete _autocomplete;
        [JsonIgnore]
        Dictionary<string, AssetInfo> _searchCache = new Dictionary<string, AssetInfo>();

        public void LoadAssets(bool forceRefresh = false)
        {
            _loaded = false;
            _cachedAssets.Clear();
            if (!forceRefresh && File.Exists(CacheFilePath))
            {
                var cacheDataString = File.ReadAllText(CacheFilePath);
                _cachedAssets = JsonConvert.DeserializeObject<List<AssetInfo>>(cacheDataString);
                _lastLoaded = DateTime.Now;
                PopulateCaches();
                _loaded = true;
                return;
            }

            var folders = Directory.GetDirectories(AssetStore5);
            foreach (var folder in folders)
            {
                var publisherFolder = Path.Combine(AssetStore5, folder);
                var assets = GetAssetsFor(publisherFolder);
                _cachedAssets.AddRange(assets);
            }

            _lastLoaded = DateTime.Now;
            var cacheData = JsonConvert.SerializeObject(_cachedAssets);
            var destination = CacheFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.WriteAllText(destination, cacheData);
            PopulateCaches();
            _loaded = true;
        }

        void PopulateCaches()
        {
            _searchCache ??= new Dictionary<string, AssetInfo>();
            _searchCache.Clear();
            foreach (var asset in _cachedAssets) _searchCache[asset.Name+" "+asset.Publisher] = asset;
            _autocomplete = Autocomplete.Create(_cachedAssets.Select(x => x.Name+" "+x.Publisher).ToList());
        }


        public List<AssetInfo> SearchAll(string search)
        {
            if (_autocomplete == null) PopulateCaches();

            if (_autocomplete == null) return null;
            var results = _autocomplete.Search(search);
            return results.Count == 0 ? null : results.Select(x => _searchCache[x.Word]).ToList();
        }

        string CacheFilePath => Path.Combine(Application.persistentDataPath, "Cache/assets.json");

        public static string AssetStore5 =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Unity/Asset Store-5.x");

        public List<AssetInfo> GetAssetsFor(string publisherFolder)
        {
            List<AssetInfo> infos = new List<AssetInfo>();
            var subFolders = Directory.GetDirectories(publisherFolder);
            foreach (var cat in subFolders)
            {
                var categoryFolder = Path.Combine(publisherFolder, cat);
                var assetFiles = Directory.GetFiles(categoryFolder, "*.unitypackage");
                foreach (var assetFile in assetFiles) infos.Add(AssetInfo.Create(assetFile));
            }

            return infos;
        }

        public IEnumerator<AssetInfo> GetEnumerator() => _cachedAssets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    [Serializable]
    public class AssetInfo
    {
        public static AssetInfo Create(string fullAssetPath)
        {
            var fileName = Path.GetFileName(fullAssetPath);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fullAssetPath);
            var parentFolder = Directory.GetParent(fullAssetPath).Name;
            var grandParentFolder = Directory.GetParent(Directory.GetParent(fullAssetPath).FullName).Name;
            return new AssetInfo
            {
                Publisher = grandParentFolder,
                Category = parentFolder,
                FileName = fileName,
                Name = nameWithoutExtension
            };
        }

        public static AssetInfo Create(string publisherFolder, string categoryFolder, string assetFile)
        {
            var assetInfo = new AssetInfo
            {
                Publisher = publisherFolder,
                Category = categoryFolder,
                FileName = assetFile,
                Name = assetFile.Replace(".unitypackage", "")
            };
            return assetInfo;
        }

        public string Name;
        public string FileName;
        public string Category;
        public string Publisher;
    }
}