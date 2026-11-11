using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CasualGame
{
    /// <summary>
    /// 精灵引用计数信息
    /// </summary>
    public class SpriteRefInfo : IReference
    {
        public List<Sprite> Sprites { get; set; }

        public void Clear()
        {
            Sprites.Clear();
            Sprites = null;
        }
    }

    /// <summary>
    /// Atlas管理器，集成了图片加载、卸载和取消令牌的统一管理功能
    /// </summary>
    public class AtlasLoader
    {
        public const string SpritePath = "Assets/Game/Sprites/";
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, SpriteRefInfo> _loadedSprites; // 使用引用计数
        private readonly Dictionary<int, string> _instanceSpritePaths; // 实例化精灵的资源路径
        private bool _disposed = false;

        public AtlasLoader()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _loadedSprites = new Dictionary<string, SpriteRefInfo>();
            _instanceSpritePaths = new Dictionary<int, string>();
        }

        /// <summary>
        /// 通过图集名称和精灵名称加载图片
        /// </summary>
        /// <param name="atlasName">图集名称</param>
        /// <param name="spriteName">精灵名称</param>
        /// <returns>加载的精灵，失败返回null</returns>
        public void LoadSprite(UnityEngine.UI.Image image, string atlasName, string spriteName, Action onComplete = null)
        {
            string assetPath = SpritePath + atlasName + "/" + spriteName + ".png";
            LoadSprite(image, assetPath, onComplete);
        }

        /// <summary>
        /// 通过图集名称和精灵名称加载图片
        /// </summary>
        /// <param name="atlasName">图集名称</param>
        /// <param name="spriteName">精灵名称</param>
        /// <returns>加载的精灵，失败返回null</returns>
        public void LoadSprite(SpriteRenderer renderer, string atlasName, string spriteName, Action onComplete = null)
        {
            string assetPath = SpritePath + atlasName + "/" + spriteName + ".png";
            LoadSprite(renderer, assetPath, onComplete);
        }

        /// <summary>
        /// 通过资源路径加载图片
        /// </summary>
        /// <param name="atlasName">图集名称</param>
        /// <param name="spriteName">精灵名称</param>
        /// <returns>加载的精灵，失败返回null</returns>
        public async void LoadSprite(UnityEngine.UI.Image image, string assetPath, Action onComplete = null)
        {
            int instanceId = (int)EntityId.ToULong(image.GetEntityId());
            _instanceSpritePaths[instanceId] = assetPath;
            image.enabled = false;
            Sprite sprite = await LoadSprite(assetPath);
            if (_instanceSpritePaths.TryGetValue(instanceId, out string path) && path.Equals(assetPath))
            {
                image.sprite = sprite;
                image.enabled = true;
            }
            onComplete?.Invoke();
        }

        /// <summary>
        /// 通过资源路径加载图片
        /// </summary>
        /// <param name="atlasName">图集名称</param>
        /// <param name="spriteName">精灵名称</param>
        /// <returns>加载的精灵，失败返回null</returns>
        public async void LoadSprite(SpriteRenderer renderer, string assetPath, Action onComplete = null)
        {
            int instanceId = (int)EntityId.ToULong(renderer.GetEntityId());
            _instanceSpritePaths[instanceId] = assetPath;
            renderer.enabled = false;
            Sprite sprite = await LoadSprite(assetPath);
            if (_instanceSpritePaths.TryGetValue(instanceId, out string path) && path.Equals(assetPath))
            {
                renderer.sprite = sprite;
                renderer.enabled = true;
            }
            onComplete?.Invoke();
        }

        /// <summary>
        /// 通过资源路径加载图片
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>加载的精灵，失败返回null</returns>
        private async UniTask<Sprite> LoadSprite(string assetPath)
        {
            if (_disposed || _cancellationTokenSource.Token.IsCancellationRequested)
            {
                LogHandler.Info("AtlasLoader: 管理器已被取消或释放，无法加载图片", LogColor.Yellow);
                return null;
            }

            if (!assetPath.StartsWith(SpritePath))
            {
                LogHandler.Error($"AtlasLoader: 资源路径不正确 {assetPath}");
                return null;
            }

            if (_loadedSprites.TryGetValue(assetPath, out SpriteRefInfo refInfo))
            {
                if (refInfo.Sprites != null && refInfo.Sprites.Count > 0)
                {
                    return refInfo.Sprites[0];
                }

                ReferencePool.Release(refInfo);
                _loadedSprites.Remove(assetPath);
            }

            try
            {
                Sprite sprite = await Framework.GameRoot.Res.LoadAsset<Sprite>(assetPath, _cancellationTokenSource.Token);
                if (sprite != null)
                {
                    if (_loadedSprites.TryGetValue(assetPath, out SpriteRefInfo existingRef))
                    {
                        existingRef.Sprites.Add(sprite);
                    }
                    else
                    {
                        _loadedSprites[assetPath] = ReferencePool.Acquire<SpriteRefInfo>();
                        _loadedSprites[assetPath].Sprites = new List<Sprite> { sprite };
                    }
                }

                return sprite;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 取消令牌
        /// </summary>
        public void Cancel()
        {
            if (_disposed) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            UnloadAllSprites();
            LogHandler.Info("AtlasLoader: 管理器已释放", LogColor.Green);
            _disposed = true;
        }

        /// <summary>
        /// 卸载所有已加载的图片
        /// </summary>
        private void UnloadAllSprites()
        {
            foreach (var kvp in _loadedSprites)
            {
                var refInfo = kvp.Value;
                if (refInfo.Sprites != null)
                {
                    for (int i = 0; i < refInfo.Sprites.Count; i++)
                    {
                        Framework.GameRoot.Res.UnloadAsset(refInfo.Sprites[i]);
                    }
                }
                ReferencePool.Release(refInfo);
            }
            _loadedSprites.Clear();
            _instanceSpritePaths.Clear();
        }
    }
}