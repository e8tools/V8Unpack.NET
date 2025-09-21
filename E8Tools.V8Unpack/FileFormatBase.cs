/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace E8Tools.V8Unpack
{
    public class ContainerHeader
    {
        public readonly ulong FreePageAddress;
        public readonly uint PageSize;
        public readonly uint StorageVer;
        public readonly uint Reserved;

        public ContainerHeader(ulong freePageAddress, uint pageSize, uint storageVer, uint reserved)
        {
            FreePageAddress = freePageAddress;
            PageSize = pageSize;
            StorageVer = storageVer;
            Reserved = reserved;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ContainerHeaderDto64
    {
        public readonly UInt64 FreePageAddress;
        public readonly UInt32 PageSize;
        public readonly UInt32 StorageVer;
        public readonly UInt32 Reserved;

        public ContainerHeader Cast()
        {
            return new ContainerHeader(
                FreePageAddress,
                PageSize,
                StorageVer,
                Reserved
            );
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ContainerHeaderDto32
    {
        public readonly UInt32 FreePageAddress;
        public readonly UInt32 PageSize;
        public readonly UInt32 StorageVer;
        public readonly UInt32 Reserved;

        public ContainerHeader Cast()
        {
            return new ContainerHeader(
                FreePageAddress,
                PageSize,
                StorageVer,
                Reserved
            );
        }
    }

    public class ElementAddress
    {
        public readonly ulong HeaderAddress;
        public readonly ulong DataAddress;
        public readonly ulong Signature;

        public ElementAddress(ulong headerAddress, ulong dataAddress, ulong signature)
        {
            HeaderAddress = headerAddress;
            DataAddress = dataAddress;
            Signature = signature;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ElementAddressDto32
    {
        public readonly UInt32 HeaderAddress;
        public readonly UInt32 DataAddress;
        public readonly UInt32 Signature;

        public ElementAddress Cast()
        {
            return new ElementAddress(HeaderAddress, DataAddress, Signature);
        }
    }

    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ElementAddressDto64
    {
        public readonly UInt64 HeaderAddress;
        public readonly UInt64 DataAddress;
        public readonly UInt64 Signature;

        public ElementAddress Cast()
        {
            return new ElementAddress(HeaderAddress, DataAddress, Signature);
        }
    }

    public class ElementHeaderData
    {
        public readonly ulong DateCreation;
        public readonly ulong DateModification;
        public readonly uint Version;

        public readonly string Name;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ElementHeaderDataDto
    {
        public readonly UInt64 DateCreation;
        public readonly UInt64 DateModification;
        public readonly UInt32 Version;
    }

    public class BlockHeader
    {
        public readonly ulong DataSize;
        public readonly ulong PageSize;
        public readonly ulong NextPageAddr;

        public BlockHeader(ulong dataSize, ulong pageSize, ulong nextPageAddr)
        {
            DataSize = dataSize;
            PageSize = pageSize;
            NextPageAddr = nextPageAddr;
        }

        public static bool TryRead<AddressUIntType>(Stream stream, out BlockHeader header) where AddressUIntType : struct
        {
            header = null;
            
            if (!Utils.ReadCertainChar(stream, 13) || !Utils.ReadCertainChar(stream, 10)) return false;
            
            ulong DataSize = Utils.ReadUIntFromHexString<AddressUIntType>(stream);
            
            if (!Utils.ReadCertainChar(stream, 32)) return false;
            
            ulong PageSize = Utils.ReadUIntFromHexString<AddressUIntType>(stream);

            if (!Utils.ReadCertainChar(stream, 32)) return false;

            ulong NextPageAddr = Utils.ReadUIntFromHexString<AddressUIntType>(stream);

            if (!Utils.ReadCertainChar(stream, 32)) return false;
            if (!Utils.ReadCertainChar(stream, 13) || !Utils.ReadCertainChar(stream, 10)) return false;

            header = new BlockHeader(DataSize, PageSize, NextPageAddr);
            return true;
        }

        public static bool TryWrite<AddressUIntType>(Stream stream, BlockHeader header) where AddressUIntType : struct
        {
            throw new NotImplementedException();
        }
    }

    public class FormatReader
    {

        public static readonly FormatReader Instance = new FormatReader();

        public virtual ContainerAddressType AddressType { get; } = ContainerAddressType._32bit;

        public virtual ulong V8_FF_SIGNATURE { get; } = 0x7fffffff;

        public virtual ContainerHeader ReadContainerHeader(Stream stream)
        {
            return Utils.Read<ContainerHeaderDto32>(stream).Cast();
        }

        public virtual BlockHeader ReadBlockHeader(Stream stream)
        {
            if (BlockHeader.TryRead<UInt32>(stream, out BlockHeader blockHeader))
            {
                return blockHeader;
            }
            return null;
        }

        public virtual void Seek(Stream stream, ulong offset)
        {
            stream.Seek((long)offset, SeekOrigin.Begin);
        }

        public virtual ElementAddress ReadElementAddress(Stream stream)
        {
            var elementAddress = Utils.Read<ElementAddressDto32>(stream);
            return elementAddress.Cast();
        }

        public virtual void WriteBlockHeader(Stream stream, BlockHeader header)
        {
            throw new NotImplementedException();
        }

    }

    public sealed class FormatReader64 : FormatReader
    {

        public static new readonly FormatReader64 Instance = new FormatReader64();
        
        public const ulong V8_OFFSET_80316 = 0x1359;  // волшебное смещение, откуда такая цифра неизвестно...
        
        public override ulong V8_FF_SIGNATURE { get; } = 0xffffffffffffffff;

        public override ContainerAddressType AddressType { get; } = ContainerAddressType._64bit;

        public override ContainerHeader ReadContainerHeader(Stream stream)
        {
            return Utils.Read<ContainerHeaderDto64>(stream).Cast();
        }

       public override BlockHeader ReadBlockHeader(Stream stream)
        {
            if (BlockHeader.TryRead<UInt64>(stream, out BlockHeader blockHeader))
            {
                return blockHeader;
            }
            return null;
        }

        public override void Seek(Stream stream, ulong offset)
        {
            stream.Seek((long)(offset + V8_OFFSET_80316), SeekOrigin.Begin);
        }

        public override ElementAddress ReadElementAddress(Stream stream)
        {
            var elementAddress = Utils.Read<ElementAddressDto64>(stream);
            return elementAddress.Cast();
        }

        public override void WriteBlockHeader(Stream stream, BlockHeader header)
        {
            throw new NotImplementedException();
        }
    }

}
