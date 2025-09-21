/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;

namespace E8Tools.V8Unpack
{
    /// <summary>
    /// Элемент контейнера (Файл)
    /// </summary>
    public class File
    {

        private readonly Container _owner;

        internal File(Container owner, string name, DateTime creationTime, DateTime modificationTime, ulong dataOffset)
        {
            Name = name;
            ModificationTime = modificationTime;
            CreationTime = creationTime;
            DataOffset = dataOffset;
            _owner = owner;
        }

        /// <summary>
        /// Содержит имя файла в контейнере.
        /// </summary>
        /// <value>Имя файла.</value>
        public string Name { get; }

        /// <summary>
        /// Содержит Дату и время последнего изменения файла.
        /// </summary>
        /// <value>Время изменения.</value>
        public DateTime ModificationTime { get; }

        /// <summary>
        /// Содержит дату и время создания файла.
        /// </summary>
        /// <value>Время создания.</value>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Смещение блока данных в контейнере.
        /// </summary>
        /// <value>Смещение блока данных в контейнере.</value>
        public ulong DataOffset { get; }

        /// <summary>
        /// Открывает поток для чтение файла.
        /// </summary>
        /// <param name="forceDecompression">Требуется ли автоматическая распаковка данных файла</param>
        /// <returns>Поток чтения файла</returns>
        public Stream GetStream(bool forceDecompression = true)
        {
            return _owner.OpenStream(this, forceDecompression);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((File)obj);
        }

        public bool Equals(File other)
        {
            return ReferenceEquals(_owner, other._owner)
                   && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }
}