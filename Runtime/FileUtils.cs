using DataUtilities.Serializer;

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataUtilities.FilePacker
{
    enum ThingType : byte
    {
        Real,
        UserDefined,
    }

    public class PackHeader : ISerializable<PackHeader>
    {
        public bool SaveMetadata;

        public void Deserialize(Deserializer deserializer)
        {
            SaveMetadata = deserializer.DeserializeBoolean();
        }

        public void Serialize(Serializer.Serializer serializer)
        {
            serializer.Serialize(SaveMetadata);
        }
    }

    public class Packer
    {
        readonly PackHeader Header;

        public Packer(PackHeader header)
        {
            Header = header;
        }

        public byte[] Pack(string folder) => Pack(new DirectoryInfo(folder));
        public byte[] Pack(DirectoryInfo folder)
        {
            Serializer.Serializer serializer = new();
            serializer.Serialize(Header);
            PackFolder(serializer, folder);
            return serializer.Result;
        }
        public byte[] Pack(IFolder folder)
        {
            Serializer.Serializer serializer = new();
            serializer.Serialize(Header);
            PackFolder(serializer, folder);
            return serializer.Result;
        }

        public void Pack(string folder, string output) => Pack(new DirectoryInfo(folder), output);
        public void Pack(DirectoryInfo folder, string output)
        {
            Serializer.Serializer serializer = new();
            serializer.Serialize(Header);
            PackFolder(serializer, folder);
            File.WriteAllBytes(output, serializer.Result);
        }
        public void Pack(IFolder folder, string output)
        {
            Serializer.Serializer serializer = new();
            serializer.Serialize(Header);
            PackFolder(serializer, folder);
            File.WriteAllBytes(output, serializer.Result);
        }

        void PackFolder(Serializer.Serializer serializer, DirectoryInfo folder)
        {
            DirectoryInfo[] folders = folder.GetDirectories();
            FileInfo[] files = folder.GetFiles();

            serializer.Serialize(folders, SerializeFolder, INTEGER_TYPE.INT32);
            serializer.Serialize(files, SerializeFile, INTEGER_TYPE.INT32);
        }

        void SerializeFolder(Serializer.Serializer serializer, DirectoryInfo folder)
        {
            serializer.Serialize((byte)ThingType.Real);
            serializer.Serialize(folder.Name);

            if (Header.SaveMetadata)
            {
                serializer.Serialize((int)folder.Attributes);
                serializer.Serialize(folder.CreationTimeUtc.Ticks);
                serializer.Serialize(folder.LastAccessTimeUtc.Ticks);
                serializer.Serialize(folder.LastWriteTimeUtc.Ticks);
            }

            PackFolder(serializer, folder);
        }
        void SerializeFile(Serializer.Serializer serializer, FileInfo file)
        {
            serializer.Serialize((byte)ThingType.Real);
            serializer.Serialize(file.Name);
            serializer.Serialize(File.ReadAllBytes(file.FullName));
            if (Header.SaveMetadata)
            {
                serializer.Serialize((int)file.Attributes);
                serializer.Serialize(file.IsReadOnly);
                serializer.Serialize(file.CreationTimeUtc.Ticks);
                serializer.Serialize(file.LastAccessTimeUtc.Ticks);
                serializer.Serialize(file.LastWriteTimeUtc.Ticks);
            }
        }

        void PackFolder(Serializer.Serializer serializer, IFolder folder)
        {
            IFolder[] folders = folder.Folders.ToArray();
            IFile[] files = folder.Files.ToArray();

            serializer.Serialize(folders, SerializeFolder, INTEGER_TYPE.INT32);
            serializer.Serialize(files, SerializeFile, INTEGER_TYPE.INT32);
        }

        void SerializeFolder(Serializer.Serializer serializer, IFolder folder)
        {
            serializer.Serialize((byte)ThingType.UserDefined);
            serializer.Serialize(folder.Name);

            PackFolder(serializer, folder);
        }
        void SerializeFile(Serializer.Serializer serializer, IFile file)
        {
            serializer.Serialize((byte)ThingType.UserDefined);
            serializer.Serialize(file.Name);
            serializer.Serialize(File.ReadAllBytes(file.FullName));
        }
    }

    public class Unpacker
    {
        readonly Stack<DirectoryInfo> folderStack = new();
        PackHeader Header;

        string CurrentPath => folderStack.Peek().FullName;

        public static void Unpack(string file)
        {
            FileInfo fileInfo = new(file);

            string rootPath = Path.Combine(fileInfo.DirectoryName, fileInfo.Name[..^fileInfo.Extension.Length]);

            Unpack(fileInfo.FullName, rootPath);
        }
        public static void Unpack(string file, string output)
        {
            Unpacker unpacker = new();

            FileInfo fileInfo = new(file);

            if (!Directory.Exists(output))
            { unpacker.folderStack.Push(Directory.CreateDirectory(output)); }
            else
            { unpacker.folderStack.Push(new DirectoryInfo(output)); }
            Deserializer deserializer = new(File.ReadAllBytes(fileInfo.FullName));

            unpacker.Header = deserializer.DeserializeObject<PackHeader>();

            unpacker.UnpackFolder(deserializer);
        }
        void UnpackFolder(Deserializer deserializer)
        {
            deserializer.DeserializeArray(DeserializeFolder, INTEGER_TYPE.INT32);
            deserializer.DeserializeArray(DeserializeFile, INTEGER_TYPE.INT32);
        }

        DirectoryInfo DeserializeFolder(Deserializer deserializer)
        {
            ThingType thingType = (ThingType)deserializer.DeserializeByte();

            string name = deserializer.DeserializeString();
            DirectoryInfo folder = Directory.CreateDirectory(Path.Combine(CurrentPath, name));

            if (thingType == ThingType.Real && Header.SaveMetadata)
            {
                folder.Attributes = (FileAttributes)deserializer.DeserializeInt32();
                folder.CreationTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
                folder.LastAccessTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
                folder.LastWriteTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
            }

            folderStack.Push(folder);
            UnpackFolder(deserializer);
            folderStack.Pop();
            return folder;
        }
        FileInfo DeserializeFile(Deserializer deserializer)
        {
            ThingType thingType = (ThingType)deserializer.DeserializeByte();

            string name = deserializer.DeserializeString();
            byte[] content = deserializer.DeserializeArray<byte>(INTEGER_TYPE.INT32);
            string path = Path.Combine(CurrentPath, name);

            using (FileStream stream = File.Create(path))
            { stream.Write(content, 0, content.Length); }
            FileInfo file = new(path);

            if (thingType == ThingType.Real && Header.SaveMetadata)
            {
                file.Attributes = (FileAttributes)deserializer.DeserializeInt32();
                file.IsReadOnly = deserializer.DeserializeBoolean();
                file.CreationTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
                file.LastAccessTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
                file.LastWriteTimeUtc = new System.DateTime(deserializer.DeserializeInt64());
            }

            return new FileInfo(path);
        }
    }

    public interface IFileOrFolder
    {
        public string Name { get; }
        public string FullName { get; }
    }

    public class VirtualThing
    {
        internal VirtualFolder Parent;
        protected string name;

        public override string ToString() => name;
    }

    public interface IFile : IFileOrFolder
    {
        public byte[] Bytes { get; }
        public string Text { get; }
    }

    public interface IModifiableFile : IFile
    {
        public new byte[] Bytes { get; set; }
        public new string Text { get; set; }
    }

    public static class Glob
    {
        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this IFileOrFolder self, string pattern) => new System.Text.RegularExpressions.Regex(
                "^" + System.Text.RegularExpressions.Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline
            ).IsMatch(self.Name);
    }

    public class VirtualFile : VirtualThing, IFile
    {
        readonly byte[] content;

        public byte[] Bytes => content;
        public string Text => System.Text.Encoding.UTF8.GetString(content);
        public string FullName => (Parent == null) ? name : (Parent.FullName + '\\' + name);
        public string Name { get => name; set => name = value; }

        public VirtualFile(string name, byte[] content)
        {
            this.name = name;
            this.content = content;
        }
    }

    public interface IFolder : IFileOrFolder
    {
        public IEnumerable<IFile> Files { get; }
        public IEnumerable<IFolder> Folders { get; }
    }
    public interface IModifiableFolder : IFolder
    {
        public void AddFile(string fileName);
        public void AddFolder(string folderName);
    }
    public interface IModifiableFolder<TFile, TFolder> : IFolder
        where TFile : IFile
        where TFolder : IFolder
    {
        public void AddFile(TFile files);
        public void AddFolder(TFolder folders);
    }

    public class VirtualFolder : VirtualThing, IModifiableFolder<VirtualFile, VirtualFolder>
    {
        readonly List<VirtualFile> files = new();
        readonly List<VirtualFolder> folders = new();

        public IEnumerable<IFile> Files => files;
        public IEnumerable<IFolder> Folders => folders;

        public string FullName => (Parent == null) ? (name ?? "") : (Parent.FullName + '\\' + name);
        public string Name => name;

        public VirtualThing this[string name] => (VirtualThing)GetFolder(name) ?? (VirtualThing)GetFile(name);

        public VirtualFolder(string name)
        {
            this.name = name;
        }

        public VirtualFile GetFile(string name)
        {
            for (int i = 0; i < files.Count; i++)
            { if (files[i].Name == name) return files[i]; }
            return null;
        }
        public VirtualFolder GetFolder(string name)
        {
            for (int i = 0; i < folders.Count; i++)
            { if (folders[i].name == name) return folders[i]; }
            return null;
        }

        public void AddFiles(params VirtualFile[] files)
        {
            for (int i = 0; i < files.Length; i++)
            { files[i].Parent = this; }
            this.files.AddRange(files);
        }
        public void AddFolders(params VirtualFolder[] folders)
        {
            for (int i = 0; i < folders.Length; i++)
            { folders[i].Parent = this; }
            this.folders.AddRange(folders);
        }

        public void AddFile(VirtualFile file) => this.files.Add(file);
        public void AddFolder(VirtualFolder folder) => this.folders.Add(folder);
    }

    public class VirtualUnpacker
    {
        readonly Stack<VirtualFolder> folderStack = new();
        PackHeader Header;

        public static VirtualFolder Unpack(string file) => Unpack(File.ReadAllBytes(file));
        public static VirtualFolder Unpack(byte[] data)
        {
            VirtualUnpacker unpacker = new();

            unpacker.folderStack.Push(new VirtualFolder(null));
            Deserializer deserializer = new(data);

            unpacker.Header = deserializer.DeserializeObject<PackHeader>();

            unpacker.UnpackFolder(deserializer);

            return unpacker.folderStack.Pop();
        }
        void UnpackFolder(Deserializer deserializer)
        {
            folderStack.Peek().AddFolders(deserializer.DeserializeArray(DeserializeFolder, INTEGER_TYPE.INT32));
            folderStack.Peek().AddFiles(deserializer.DeserializeArray(DeserializeFile, INTEGER_TYPE.INT32));
        }

        VirtualFolder DeserializeFolder(Deserializer deserializer)
        {
            ThingType thingType = (ThingType)deserializer.DeserializeByte();
            string name = deserializer.DeserializeString();
            VirtualFolder folder = new(name);

            if (thingType == ThingType.Real && Header.SaveMetadata)
            {
                deserializer.DeserializeInt32();
                deserializer.DeserializeInt64();
                deserializer.DeserializeInt64();
                deserializer.DeserializeInt64();
            }

            folderStack.Push(folder);
            UnpackFolder(deserializer);
            folderStack.Pop();
            return folder;
        }
        VirtualFile DeserializeFile(Deserializer deserializer)
        {
            ThingType thingType = (ThingType)deserializer.DeserializeByte();
            string name = deserializer.DeserializeString();
            byte[] content = deserializer.DeserializeArray<byte>(INTEGER_TYPE.INT32);

            if (thingType == ThingType.Real && Header.SaveMetadata)
            {
                deserializer.DeserializeInt32();
                deserializer.DeserializeBoolean();
                deserializer.DeserializeInt64();
                deserializer.DeserializeInt64();
                deserializer.DeserializeInt64();
            }

            return new VirtualFile(name, content);
        }
    }
}
