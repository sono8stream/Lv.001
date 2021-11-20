using System;

namespace Expression.Map.MapTile
{
    class TileData
    {
        public string SettingName { get; private set; }

        public string BaseTileFilePath { get; private set; }

        public string[] AutoTileFilePaths { get; private set; }

        public UnitTile[] UnitTileConfigs { get; private set; }

        public TileData(string settingName, string baseTileFilePath, string[] autoTileFilePaths, UnitTile[] unitTileConfigs)
        {
            SettingName = settingName;
            BaseTileFilePath = baseTileFilePath;
            AutoTileFilePaths = autoTileFilePaths;
            UnitTileConfigs = unitTileConfigs;
        }
    }
}