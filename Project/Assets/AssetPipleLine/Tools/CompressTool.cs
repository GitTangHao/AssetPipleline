using UnityEngine;

using System;
using System.IO;
using System.Collections;

using LZ4;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;


public class CompressTool
{
    public static void CompressLZMAFile(string a_inputFile, string a_outputFile)
    {
        SevenZip.Compression.LZMA.Encoder tCoder = new SevenZip.Compression.LZMA.Encoder();
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create);

        tCoder.WriteCoderProperties(tOutFileStream);

        tOutFileStream.Write(BitConverter.GetBytes(tInFileStream.Length), 0, 8);

        // Encode the file.
        tCoder.Code(tInFileStream, tOutFileStream, tInFileStream.Length, -1, null);
        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();
    }

    public static void DecompressLZMAFile(string a_inputFile, string a_outputFile)
    {
        SevenZip.Compression.LZMA.Decoder tCoder = new SevenZip.Compression.LZMA.Decoder();
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create);

        // Read the decoder properties
        byte[] tProperties = new byte[5];
        tInFileStream.Read(tProperties, 0, 5);

        // Read in the decompress file size.
        byte[] tFileLengthBytes = new byte[8];
        tInFileStream.Read(tFileLengthBytes, 0, 8);
        long tFileLength = BitConverter.ToInt64(tFileLengthBytes, 0);

        // Decompress the file.
        tCoder.SetDecoderProperties(tProperties);
        tCoder.Code(tInFileStream, tOutFileStream, tInFileStream.Length, tFileLength, null);
        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();
    }

    public static void CompressGZipFile(string a_inputFile, string a_outputFile)
    {
        byte[] tDataBuffer = new byte[4096];

        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);
        using (Stream tGZipStream = new GZipOutputStream(tOutFileStream))
        using (FileStream tInFileStream = File.OpenRead(a_inputFile))
        {
            StreamUtils.Copy(tInFileStream, tGZipStream, tDataBuffer);
        }
    }

    public static void DecompressGZipFile(string a_inputFile, string a_outputFile)
    {
        byte[] tDataBuffer = new byte[4096];
        
        using (Stream tGZipStream = new GZipInputStream(File.OpenRead(a_inputFile)))
        using (FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
        {
            StreamUtils.Copy(tGZipStream, tOutFileStream, tDataBuffer);
        }
    }

    public static void CompressLZ4File(string a_inputFile, string a_outputFile)
    {
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);
        
        byte[] tOriginal = new byte[tInFileStream.Length];
        tInFileStream.Read(tOriginal, 0, tOriginal.Length);

        byte[] tCompressed = LZ4Codec.Wrap(tOriginal, 0, tOriginal.Length);
        tOutFileStream.Write(tCompressed, 0, tCompressed.Length);

        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();
    }

    public static void DecompressLZ4File(string a_inputFile, string a_outputFile)
    {
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);

        byte[] tCompressed = new byte[tInFileStream.Length];
        tInFileStream.Read(tCompressed, 0, tCompressed.Length);

        byte[] tOriginal = LZ4Codec.Unwrap(tCompressed, 0);

        tOutFileStream.Write(tOriginal, 0, tOriginal.Length);

        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();

    }


	/*
    public static void CompresssSnappyFile(string a_inputFile, string a_outputFile)
    {
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);

        byte[] tOriginal = new byte[tInFileStream.Length];
        tInFileStream.Read(tOriginal, 0, tOriginal.Length);

        byte[] tCompressed = Snappy.SnappyCodec.Compress(tOriginal);
        tOutFileStream.Write(tCompressed, 0, tCompressed.Length);

        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();
    }

    public static void DecompressSnappyFile(string a_inputFile, string a_outputFile)
    {
        FileStream tInFileStream = new FileStream(a_inputFile, FileMode.Open, FileAccess.Read);
        FileStream tOutFileStream = new FileStream(a_outputFile, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);

        byte[] tCompressed = new byte[tInFileStream.Length];
        tInFileStream.Read(tCompressed, 0, tCompressed.Length);

        byte[] tOriginal = Snappy.SnappyCodec.Uncompress(tCompressed);
        tOutFileStream.Write(tOriginal, 0, tOriginal.Length);

        tOutFileStream.Flush();
        tOutFileStream.Close();
        tInFileStream.Close();
    }

	*/

}
