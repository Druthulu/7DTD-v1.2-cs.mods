using System;
using System.Collections.Generic;
using System.IO;
using SharpEXR.AttributeTypes;

namespace SharpEXR
{
	public class EXRFile
	{
		public EXRVersion Version { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public List<EXRHeader> Headers { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public List<OffsetTable> OffsetTables { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public List<EXRPart> Parts { get; [PublicizedFrom(EAccessModifier.Protected)] set; }

		public void Read(IEXRReader reader)
		{
			if (reader.ReadInt32() != 20000630)
			{
				throw new EXRFormatException("Invalid or corrupt EXR layout: First four bytes were not 20000630.");
			}
			int value = reader.ReadInt32();
			this.Version = new EXRVersion(value);
			this.Headers = new List<EXRHeader>();
			if (this.Version.IsMultiPart)
			{
				for (;;)
				{
					EXRHeader exrheader = new EXRHeader();
					exrheader.Read(this, reader);
					if (exrheader.IsEmpty)
					{
						break;
					}
					this.Headers.Add(exrheader);
				}
				throw new NotImplementedException("Multi part EXR files are not currently supported");
			}
			if (this.Version.IsSinglePartTiled)
			{
				throw new NotImplementedException("Tiled EXR files are not currently supported");
			}
			EXRHeader exrheader2 = new EXRHeader();
			exrheader2.Read(this, reader);
			this.Headers.Add(exrheader2);
			this.OffsetTables = new List<OffsetTable>();
			foreach (EXRHeader exrheader3 in this.Headers)
			{
				int num;
				if (this.Version.IsMultiPart)
				{
					num = exrheader3.ChunkCount;
				}
				else if (this.Version.IsSinglePartTiled)
				{
					num = 0;
				}
				else
				{
					EXRCompression compression = exrheader3.Compression;
					Box2I dataWindow = exrheader3.DataWindow;
					int scanLinesPerBlock = EXRFile.GetScanLinesPerBlock(compression);
					num = (int)Math.Ceiling((double)dataWindow.Height / (double)scanLinesPerBlock);
				}
				OffsetTable offsetTable = new OffsetTable(num);
				offsetTable.Read(reader, num);
				this.OffsetTables.Add(offsetTable);
			}
		}

		public static int GetScanLinesPerBlock(EXRCompression compression)
		{
			switch (compression)
			{
			case EXRCompression.ZIP:
			case EXRCompression.PXR24:
				return 16;
			case EXRCompression.PIZ:
			case EXRCompression.B44:
			case EXRCompression.B44A:
				return 32;
			default:
				return 1;
			}
		}

		public static int GetBytesPerPixel(ImageDestFormat format)
		{
			switch (format)
			{
			case ImageDestFormat.RGB8:
			case ImageDestFormat.BGR8:
				return 3;
			case ImageDestFormat.RGBA8:
			case ImageDestFormat.PremultipliedRGBA8:
			case ImageDestFormat.BGRA8:
			case ImageDestFormat.PremultipliedBGRA8:
				return 4;
			case ImageDestFormat.RGB16:
			case ImageDestFormat.BGR16:
				return 6;
			case ImageDestFormat.RGBA16:
			case ImageDestFormat.PremultipliedRGBA16:
			case ImageDestFormat.BGRA16:
			case ImageDestFormat.PremultipliedBGRA16:
				return 8;
			case ImageDestFormat.RGB32:
			case ImageDestFormat.BGR32:
				return 12;
			case ImageDestFormat.RGBA32:
			case ImageDestFormat.PremultipliedRGBA32:
			case ImageDestFormat.BGRA32:
			case ImageDestFormat.PremultipliedBGRA32:
				return 16;
			default:
				throw new ArgumentException("Unrecognized destination format", "format");
			}
		}

		public static int GetBitsPerPixel(ImageDestFormat format)
		{
			switch (format)
			{
			case ImageDestFormat.RGB8:
			case ImageDestFormat.RGBA8:
			case ImageDestFormat.PremultipliedRGBA8:
			case ImageDestFormat.BGR8:
			case ImageDestFormat.BGRA8:
			case ImageDestFormat.PremultipliedBGRA8:
				return 8;
			case ImageDestFormat.RGB16:
			case ImageDestFormat.RGBA16:
			case ImageDestFormat.PremultipliedRGBA16:
			case ImageDestFormat.BGR16:
			case ImageDestFormat.BGRA16:
			case ImageDestFormat.PremultipliedBGRA16:
				return 16;
			case ImageDestFormat.RGB32:
			case ImageDestFormat.RGBA32:
			case ImageDestFormat.PremultipliedRGBA32:
			case ImageDestFormat.BGR32:
			case ImageDestFormat.BGRA32:
			case ImageDestFormat.PremultipliedBGRA32:
				return 32;
			default:
				throw new ArgumentException("Unrecognized destination format", "format");
			}
		}

		public static EXRFile FromFile(string file)
		{
			EXRReader exrreader = new EXRReader(new FileStream(file, FileMode.Open, FileAccess.Read), false);
			EXRFile result = EXRFile.FromReader(exrreader);
			exrreader.Dispose();
			return result;
		}

		public static EXRFile FromStream(Stream stream)
		{
			EXRReader exrreader = new EXRReader(new BinaryReader(stream));
			EXRFile result = EXRFile.FromReader(exrreader);
			exrreader.Dispose();
			return result;
		}

		public static EXRFile FromReader(IEXRReader reader)
		{
			EXRFile exrfile = new EXRFile();
			exrfile.Read(reader);
			exrfile.Parts = new List<EXRPart>();
			for (int i = 0; i < exrfile.Headers.Count; i++)
			{
				EXRPart item = new EXRPart(exrfile.Version, exrfile.Headers[i], exrfile.OffsetTables[i]);
				exrfile.Parts.Add(item);
			}
			return exrfile;
		}
	}
}
