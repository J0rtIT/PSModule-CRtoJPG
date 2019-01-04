using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;


namespace CRtoJPG
{
    public class Converter
    {
        public static byte[] Buffer1 { get; } = new byte[BufferSize];
        private const int BufferSize = 512 * 1024;
        private static readonly ImageCodecInfo JpgImageCodec = GetJpegCodec();

        public static void ConvertImage(string fileName, string outputFolder)
        {
            try
            {
                using (FileStream fi = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                    BufferSize, FileOptions.None))
                {
                    // Start address is at offset 0x62, file size at 0x7A, orientation at 0x6E
                    fi.Seek(0x62, SeekOrigin.Begin);
                    BinaryReader br = new BinaryReader(fi);
                    UInt32 jpgStartPosition = br.ReadUInt32(); // 62
                    br.ReadUInt32(); // 66
                    br.ReadUInt32(); // 6A
                    UInt32 orientation = br.ReadUInt32() & 0x000000FF; // 6E
                    br.ReadUInt32(); // 72
                    br.ReadUInt32(); // 76
                    Int32 fileSize = br.ReadInt32(); // 7A

                    fi.Seek(jpgStartPosition, SeekOrigin.Begin);

                    string baseName = Path.GetFileNameWithoutExtension(fileName);
                    string jpgName = Path.Combine(outputFolder, baseName + ".jpg");

                    Bitmap bitmap = new Bitmap(new PartialStream(fi, jpgStartPosition, fileSize));

                    try
                    {
                        if (JpgImageCodec != null && (orientation == 8 || orientation == 6))
                        {
                            if (orientation == 8)
                                bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                            else
                                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Image Skipped
                        Console.WriteLine($"The image {fileName} will be skipped\nMessage: {ex.Message}");

                    }

                    EncoderParameters ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                    bitmap.Save(jpgName, JpgImageCodec, ep);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"there was an error while trying to convert the file {fileName} with message\n{ex.Message}");
            }
        }

        private static ImageCodecInfo GetJpegCodec()
        {
            foreach (ImageCodecInfo c in ImageCodecInfo.GetImageEncoders())
            {
                if (c.CodecName.ToLower().Contains("jpeg")
                    || c.FilenameExtension.ToLower().Contains("*.jpg")
                    || c.FormatDescription.ToLower().Contains("jpeg")
                    || c.MimeType.ToLower().Contains("image/jpeg"))
                    return c;
            }

            return null;
        }
    }
}
