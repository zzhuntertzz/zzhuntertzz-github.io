using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;

public static partial class MyKeys
{
    public static partial class Package
    {
        public static readonly string No_Ad = "no_ad";
    }
    
    public static partial class Material
    {
        public static readonly string Flash = "flash";
    }
    public static partial class Shader
    {
        public static readonly string Flash = "flash_shader";
    }
    
    public static partial class Tags
    {
    }
    
    public static partial class Layer
    {
    }
    
    public static partial class Datas
    {
        public static string LevelData(int levelId) => $"data_{levelId}";
    }
    
    public static partial class Prefabs
    {
        public static readonly string AppsFlyerObject = "AppsFlyerObject";
        public static readonly string ProductManager = "ProductManager";
        
        public static readonly string NotiText = "noti_text";
        
        public static readonly string Level_Sample = $"{nameof(Level_Sample)}";
    }
    
    public static partial class Sounds
    {
        public static readonly string ClickButton = "Click Button";
    }

    public static partial class Atlas
    {
        public static readonly string Currency = nameof(Currency);
        public static async UniTask<Sprite> GetSprite(string atlas, string sprite)
        {
            var atlasFile = await A.Get<SpriteAtlas>(atlas);
            if (!atlasFile) return null;
            var spriteFile = atlasFile.GetSprite(sprite);
            if (!spriteFile) return null;
            return spriteFile;
        }
    }
    
    public static partial class GSheet
    {
        public static partial class Sheet
        {
            public static readonly string GameData = "";
        }

        public static partial class Page
        {
            public static readonly int ShopIAP = 0;
            public static readonly int Config = 0;
            public static readonly int Currency = 0;
            public static readonly int Localize = 0;
        }
    }
}