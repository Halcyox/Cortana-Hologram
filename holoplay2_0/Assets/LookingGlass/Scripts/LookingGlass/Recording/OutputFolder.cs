﻿// Thanks to https://github.com/needle-mirror/com.unity.recorder/blob/master/Editor/Sources/OutputPath.cs
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// A helper class that allows building file paths relative to certain special Unity folders.
    /// </summary>
    [Serializable]
    internal class OutputFolder : ISerializationCallbackReceiver {
        /// <summary>
        /// Options specifying which root location the output path is relative to (or if the path is absolute).
        /// </summary>
        public enum DirectoryRootType {
            /// <summary>A relative path to Project folder (the parent directory, above Assets).</summary>
            Project,

            /// <summary>A relative path to your Unity project's Assets folder.</summary>
            AssetsFolder,

            /// <summary>A relative path to Unity's StreamingAssets folder.</summary>
            StreamingAssets,

            /// <summary>A relative path to Unity's persistent data path.</summary>
            PersistentData,

            /// <summary>A relative path to Unity's temporary cache folder.</summary>
            TemporaryCache,

            /// <summary>An absolute path.</summary>
            Absolute
        }

        [SerializeField] internal DirectoryRootType root = DirectoryRootType.Project;
        [SerializeField] internal string value = "Recordings";
        [SerializeField] internal bool forceAssetFolder = false;

        public DirectoryRootType Root {
            get { return root; }
            set { root = value; }
        }

        public string Value {
            get { return value; }
            set { this.value = value; }
        }

        public bool ForceAssetsFolder {
            get { return forceAssetFolder; }
            set {
                forceAssetFolder = value;

                if (forceAssetFolder)
                    root = DirectoryRootType.AssetsFolder;
            }
        }

        private static string GetForwardPath(string path) => path.Replace('\\', '/');

        public static OutputFolder FromPath(string path) {
            OutputFolder result = new OutputFolder();

            path = GetForwardPath(path);
            string unityPath;

            if (path.Contains(unityPath = GetForwardPath(Application.streamingAssetsPath))) {
                result.root = DirectoryRootType.StreamingAssets;
                result.value = path.Replace(unityPath, string.Empty);
            } else if (path.Contains(unityPath = GetForwardPath(Application.dataPath))) {
                result.root = DirectoryRootType.AssetsFolder;
                result.value = path.Replace(unityPath, string.Empty);
            } else if (path.Contains(unityPath = GetForwardPath(Application.persistentDataPath))) {
                result.root = DirectoryRootType.PersistentData;
                result.value = path.Replace(unityPath, string.Empty);
            } else if (path.Contains(unityPath = GetForwardPath(Application.temporaryCachePath))) {
                result.root = DirectoryRootType.TemporaryCache;
                result.value = path.Replace(unityPath, string.Empty);
            } else if (path.Contains(unityPath = ProjectPath())) {
                result.root = DirectoryRootType.Project;
                result.value = path.Replace(unityPath, string.Empty);
            } else {
                result.root = DirectoryRootType.Absolute;
                result.value = path;
            }

            if (result.root != DirectoryRootType.Absolute)
                result.value = result.value.TrimStart('/');

            return result;
        }

        public static string GetFullPath(DirectoryRootType root, string value) {
            if (root == DirectoryRootType.Absolute)
                return value;

            string result = string.Empty;
            switch (root) {
                case DirectoryRootType.PersistentData:
                    result = GetForwardPath(Application.persistentDataPath);
                    break;
                case DirectoryRootType.StreamingAssets:
                    result = GetForwardPath(Application.streamingAssetsPath);
                    break;
                case DirectoryRootType.TemporaryCache:
                    result = GetForwardPath(Application.temporaryCachePath);
                    break;
                case DirectoryRootType.AssetsFolder:
                    result = GetForwardPath(Application.dataPath);
                    break;
                case DirectoryRootType.Project:
                    result = ProjectPath();
                    break;
            }

            if (!value.StartsWith("/"))
                result += "/";
            result += value;
            return result;
        }

        public string GetFullPath() => GetFullPath(root, value).Replace('\\', '/');

        public static string ProjectPath() {
            return Regex.Replace(Application.dataPath, "/Assets$", string.Empty).Replace('\\', '/');
        }

        public void OnBeforeSerialize() {
            value = PathUtil.SanitizeFilePath(value);
        }

        public void OnAfterDeserialize() { }
    }
}
