using System;

namespace Infrastructure.Map.Util.MapTile
{
    class Data
    {
        public string SettingName { get; private set; }

        public string BaseTileFilePath { get; private set; }

        public string[] AutoTileFilePaths { get; private set; }

        public UnitTile[] UnitTileConfigs { get; private set; }

        public Data(string settingName, string baseTileFilePath, string[] autoTileFilePaths, UnitTile[] unitTileConfigs)
        {
            SettingName = settingName;
            BaseTileFilePath = baseTileFilePath;
            AutoTileFilePaths = autoTileFilePaths;
            UnitTileConfigs = unitTileConfigs;
        }
    }
}