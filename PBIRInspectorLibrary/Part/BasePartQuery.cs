﻿using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PBIRInspectorLibrary.Part
{
    internal class BasePartQuery(string fileSystemPath) : IPartQuery
    {
        private const string UNIQUEPARTMETHODNAME = "UniquePart";
        private const string NAMEPOINTER = "/name";
        private const string DISPLAYNAMEPOINTER = "/displayName";

        public Part RootPart { get; set; }

        public virtual object? Invoke(string query, Part context)
        {
            object? result = null;
            if (string.IsNullOrEmpty(query)) return context;

            var type = this.GetType();
            System.Reflection.MethodInfo? mi = type.GetMethod(query);
            if (mi != null)
            {
                //TODO: retrict invokable methods to their own namespace?
                result = mi.Invoke(this, new object?[] { context });
            }
            else
            {
                result = SearchParts(query, context);
            }

            return result;
        }

        private protected virtual object? SearchParts(string query, Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                  where Regex.IsMatch(p.FileSystemPath, query, RegexOptions.IgnoreCase)
                                  select p;

            if (q is null) return null;
            
            return q.ToList();
        }

        private protected void SetParts()
        {
            if (this.RootPart == null) throw new ArgumentNullException("RootPart is not set.");
            SetParts(this.RootPart);
        }

        private protected void SetParts(Part context)
        {
            if (this.RootPart == null)
            {
                this.RootPart = context;
            }
            if (!Directory.Exists(context.FileSystemPath)) return;

            context.Parts = new List<Part>();

            foreach (string filePath in Directory.GetFiles(context.FileSystemPath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                Part filePart = new Part(fileInfo.Name, fileInfo.FullName, context, PartFileSystemTypeEnum.File);
                context.Parts.Add(filePart);
            }

            foreach (string dirPath in Directory.GetDirectories(context.FileSystemPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                Part dirPart = new Part(dirInfo.Name, dirInfo.FullName, context, PartFileSystemTypeEnum.Folder);
                context.Parts.Add(dirPart);
                SetParts(dirPart);
            }
        }

        #region Methods invokeable from rules 
        public Part Parent(Part context)
        {
            return context.Parent;
        }

        public Part TopParent(Part context)
        {
            if (context.Parent == null) return context;
            return TopParent(context.Parent);
        }

        public Part Platform(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                  where p.PartFileSystemType == PartFileSystemTypeEnum.File && p.FileSystemName.EndsWith(".platform")
                                  select p;

            return q.Single();
        }

        //TODO: implement for folders
        public string PartName(Part context)
        {
            string? val = null;

            if (context.PartFileSystemType == PartFileSystemTypeEnum.File && context.FileSystemName.EndsWith(".json"))
            {
                var node = PartUtils.ToJsonNode(context);
                val = PartUtils.TryGetJsonNodeStringValue(node, NAMEPOINTER);
            }

            return val ?? context.FileSystemName;
        }

        //TODO: implement for folders
        public string PartDisplayName(Part context)
        {
            string? val = null;

            if (context.PartFileSystemType == PartFileSystemTypeEnum.File && context.FileSystemName.EndsWith(".json"))
            {
                var node = PartUtils.ToJsonNode(context);
                val = PartUtils.TryGetJsonNodeStringValue(node, DISPLAYNAMEPOINTER);
            }

            return val ?? context.FileSystemName;
        }

        public string PartFileSystemName(Part context)
        {
            return context.FileSystemName;
        }

        public string PartFileSystemPath(Part context)
        {
            return context.FileSystemPath;
        }

        public string PartFileExtension(Part context)
        {
            if (context.PartFileSystemType != PartFileSystemTypeEnum.File)
            {
                return null;
            }
            return Path.GetExtension(context.FileSystemName);
        }

        public string PartFileSystemType(Part context)
        {
            return context.PartFileSystemType.ToString();
        }

        public List<Part> Files(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartFileSystemType == PartFileSystemTypeEnum.File ? context.Parent : context)
                                  where p.PartFileSystemType == PartFileSystemTypeEnum.File
                                  select p;

            return q.ToList();
        }
        #endregion
    }
}