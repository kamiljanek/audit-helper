using System.Drawing;
using System.Drawing.Imaging;
using Tesseract;

namespace audit_helper;

public class ImageReader
{
    private readonly string _tessDataPath;
    // private const string _tessDataPath = @"C:\repos\_PRIVATE\audit-helper\invoices\tessdata";
    private const string _alphabetPl = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ąĄćĆęĘłŁńŃóÓśŚźŹżŻ";
    private const string _alphabetEn = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
    private const string _alphabetDe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 äÄöÖüÜß";
    private const string _alphabetCz = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 áÁčČďĎéÉěĚíÍňŇóÓřŘšŠťŤúÚůŮýÝžŽ";
    private const string _languagePl = "pol";
    private const string _languageEn = "eng";
    private const string _languageDe = "deu";
    private const string _languageCz = "ces";

    public ImageReader()
    {
        var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
        _tessDataPath = Path.Combine(exeDirectory, "tessdata");
    }

    public string GetTextDefault(string fileName)
    {
        using var engine = new TesseractEngine(_tessDataPath, "pol", EngineMode.TesseractAndLstm);

        using var img = Pix.LoadFromFile(fileName);
        using var page = engine.Process(img);

        var result = page.GetText();

        return result;
    }

    public string GetTextPl(Bitmap processedImage)
    {
        var result = GetTextByLanguage(processedImage, _languagePl, _alphabetPl);
        return result;
    }

    public string GetTextEn(Bitmap processedImage)
    {
        var result = GetTextByLanguage(processedImage, _languageEn, _alphabetEn);
        return result;
    }

    public string GetTextDe(Bitmap processedImage)
    {
        var result = GetTextByLanguage(processedImage, _languageDe, _alphabetDe);
        return result;
    }

    public string GetTextCz(Bitmap processedImage)
    {
        var result = GetTextByLanguage(processedImage, _languageCz, _alphabetCz);
        return result;
    }

    public Bitmap PreprocessImage(string imagePath)
    {
        Bitmap originalImage = new Bitmap(imagePath);

        Bitmap nonIndexedImage = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format24bppRgb);
        using (Graphics g = Graphics.FromImage(nonIndexedImage))
        {
            g.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
        }

        Bitmap grayImage = new Bitmap(nonIndexedImage.Width, nonIndexedImage.Height, PixelFormat.Format24bppRgb);
        using (Graphics g = Graphics.FromImage(grayImage))
        {
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {0.3f, 0.3f, 0.3f, 0, 0},
                    new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                    new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);

            g.DrawImage(nonIndexedImage, new Rectangle(0, 0, nonIndexedImage.Width, nonIndexedImage.Height),
                        0, 0, nonIndexedImage.Width, nonIndexedImage.Height, GraphicsUnit.Pixel, attributes);
        }

        Bitmap binaryImage = new Bitmap(grayImage.Width, grayImage.Height);
        for (int y = 0; y < grayImage.Height; y++)
        {
            for (int x = 0; x < grayImage.Width; x++)
            {
                Color pixelColor = grayImage.GetPixel(x, y);
                int grayValue = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                if (grayValue < 128)
                {
                    binaryImage.SetPixel(x, y, Color.Black);
                }
                else
                {
                    binaryImage.SetPixel(x, y, Color.White);
                }
            }
        }

        return binaryImage;
    }

    private string GetTextByLanguage(Bitmap processedImage, string language, string charWhitelist)
    {
        using var engine = new TesseractEngine(_tessDataPath, language, EngineMode.TesseractAndLstm);
        engine.SetVariable("tessedit_char_whitelist", charWhitelist);
        engine.SetVariable("user_defined_dpi", "300");

        using var img = BitmapToPix(processedImage);
        using var page = engine.Process(img);

        return page.GetText();
    }

    private Pix BitmapToPix(Bitmap bitmap)
    {
        int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;

        Pix pix = Pix.Create(bitmap.Width, bitmap.Height, bytesPerPixel * 8);

        BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

        int wpl = (bitmap.Width * bytesPerPixel + 3) / 4;  // Each word is 4 bytes

        try
        {
            int stride = bitmapData.Stride;
            int height = bitmap.Height;
            int width = bitmap.Width * bytesPerPixel;
            byte[] tempBuffer = new byte[width];

            for (int y = 0; y < height; y++)
            {
                IntPtr srcPtr = new IntPtr(bitmapData.Scan0.ToInt64() + y * stride);
                IntPtr destPtr = new IntPtr(pix.GetData().Data.ToInt64() + y * wpl * 4);

                System.Runtime.InteropServices.Marshal.Copy(srcPtr, tempBuffer, 0, width);
                System.Runtime.InteropServices.Marshal.Copy(tempBuffer, 0, destPtr, width);
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return pix;
    }
}