/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using E8Tools.V8Unpack.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace E8Tools.V8Unpack
{
    /// <summary>
    /// Работа с контейнером
    /// </summary>
    public class Container : IDisposable
    {
        private readonly bool _ownedStream;
        private readonly ContainerHeader _header;
        private readonly List<ElementAddress> _elements;
        private readonly FormatReader formatReader;

        private Container(Stream stream, bool ownedStream = false) {

            if (!stream.CanSeek)
            {
                throw new StreamMustCanSeekException();
            }

            var root = ContainerRoot.FindRoot(stream) ?? throw new NotAContainerException();
            _header = root._header;
            _elements = root._elements;
            formatReader = root._formatReader;

            Stream = stream;
            _ownedStream = ownedStream;
        }

        private Stream Stream { get; }

        /// <summary>
        /// Определяет тип адресации внутри контейнера
        /// </summary>
        public ContainerAddressType AddressType => formatReader.AddressType;

        /// <summary>
        /// Возвращает количество файлов в контейнере.
        /// </summary>
        public int Count => _elements.Count;

        /// <summary>
        /// Считывает информацию о файлах контейнера.
        /// </summary>
        /// <returns>Файлы контейнера</returns>
        public IEnumerable<File> Files()
        {
            foreach (var el in _elements)
            {
                formatReader.Seek(Stream, el.HeaderAddress);
                byte[] buffer = BlockReaderStream.ReadDataBlock(Stream, formatReader);
                var ms = new MemoryStream(buffer);
                ElementHeaderDataDto header = Utils.Read<ElementHeaderDataDto>(ms);
                var encoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
                var name = encoding.GetString(buffer, (int)ms.Position, (int)(ms.Length - ms.Position)).TrimEnd('\0');

                yield return new File(this,
                    name,
                    Utils.FromFileDate(header.DateCreation),
                    Utils.FromFileDate(header.DateModification),
                    el.DataAddress
                );
            }
        }

        /// <summary>
        /// Создает поток чтения данных файла контейнера.
        /// </summary>
        /// <param name="file">Файл контейнера</param>
        /// <param name="forceDecompression">Признак необходимости принудительной распаковки данных файла</param>
        /// <returns>Поток для чтения</returns>
        public Stream OpenStream(File file, bool forceDecompression = true)
        {
            if (file.DataOffset != 0 && file.DataOffset != formatReader.V8_FF_SIGNATURE)
            {
                formatReader.Seek(Stream, file.DataOffset);
                var blockReader = new BlockReaderStream(Stream, formatReader);
                
                if (blockReader.IsPacked && forceDecompression)
                {
                    return new DeflateStream(blockReader, CompressionMode.Decompress);
                }

                return blockReader;

            }
            return Stream.Null;
        }

        /// <summary>
        /// Определяет, что поток содержит в себе контейнер
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>true, если из потока можно прочитать контейнер. false, если поток не содержит контейнер</returns>
        public static bool IsContainer(Stream stream)
        {
            return ContainerRoot.FindRoot(stream) != null;
        }

        /// <summary>
        /// Создает контейнер из потока
        /// </summary>
        /// <param name="stream">Поток</param>
        /// <returns>Контейнер</returns>
        public static Container FromStream(Stream stream)
        {
            return new Container(stream);
        }

        /// <summary>
        /// Создает поток по имени файла.
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Контейнер</returns>
        public static Container FromFile(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return new Container(stream, true);
        }

        public void Dispose()
        {
            if (_ownedStream)
            {
                Stream.Dispose();
            }
        }
    }
}
