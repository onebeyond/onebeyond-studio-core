using System;
using System.Runtime.Serialization;
using EnsureThat;
using Newtonsoft.Json;

namespace OneBeyond.Studio.FileStorage.Domain.Entities;

/// <summary>
/// </summary>
public class FileRecord
{
    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="contentType"></param>
    /// <param name="size"></param>
    public FileRecord(string name, long size, string contentType)
        : this(new ID(Guid.NewGuid()), name, size, contentType)
    {
    }

    //We need this constructor in order to be able to create a file record with the Id explicitly specified
    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="contentType"></param>
    /// <param name="size"></param>
    public FileRecord(ID id, string name, long size, string contentType)
    {
        EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
        EnsureArg.IsGte(size, 0, nameof(size));
        EnsureArg.IsNotNullOrWhiteSpace(contentType, nameof(contentType));

        Id = id;
        Name = name;
        Size = size;
        ContentType = contentType;
    }

    //This private constructor is needed for EF Core to construct an entity from database table row
    private FileRecord()
    {
    }

    public ID Id { get; }

    /// <summary>
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// </summary>
    public long Size { get; private set; }

    /// <summary>
    /// </summary>
    public string ContentType { get; private set; }

    /// <summary>
    /// </summary>
    /// <param name="contentSize"></param>
    /// <param name="contentType"></param>
    public void UpdateContentInfo(long contentSize, string contentType)
    {
        EnsureArg.IsGte(contentSize, 0, nameof(contentSize));
        EnsureArg.IsNotNullOrWhiteSpace(contentType, nameof(contentType));

        Size = contentSize;
        ContentType = contentType;
    }

    /// <summary>
    /// Create a new copy of the file record (new Id will be generated)
    /// </summary>
    /// <param name="fileName">New file name</param>
    /// <returns></returns>
    public FileRecord Copy(string fileName = null)
    {
        return new FileRecord(
            string.IsNullOrWhiteSpace(fileName) ? Name : fileName,
            Size,
            ContentType);
    }

    /// <summary>
    /// </summary>
    public struct ID : IEquatable<ID>
    {
        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        public ID(Guid key)
        {
            EnsureArg.IsNotDefault(key, nameof(key));

            Key = key;
        }

        /// <summary>
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public ID Id => this;

        /// <summary>
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
            => this == obj as ID?;

        /// <summary>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(ID first, ID second)
            => !(first == second);

        /// <summary>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(ID first, ID second)
            => first.Equals(second);

        /// <summary>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ID other)
            => Key.Equals(other.Key);

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
            => Key.GetHashCode();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => Key.ToString();
    }
}
