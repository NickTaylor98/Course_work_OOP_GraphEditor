﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace kursach.ImageProcessing
{
    public static class BitmapProcessor
    {
        private static int ClampColor(double value)
        {
            return value > 255.0 ? 255 : value < 0.0 ? 0 : (int)value;
        }
        private static int ClampColor(int value)
        {
            return value > 255 ? 255 : value < 0 ? 0 : value;
        }
        private static double ChangePixel(byte data, double c)
        {
            const float oneOver255 = 1.0f / 255.0f;
            return ((data * oneOver255 - 0.5) * c + 0.5) * 255;
        }
        public static unsafe BitmapSource ConvertToGrayscale(this Bitmap bmp)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            for (int i = 0; i < bData.Height; ++i)
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * 3;
                    byte avg = (byte)((*data + *(data + 1) + *(data + 2)) / 3);
                    *data = *(data + 1) = *(data + 2) = avg;
                }
            bmp.UnlockBits(bData);
            return new Bitmap(bmp).ToBitmapImage();
        }

        public static unsafe BitmapImage ReverseImage(this Bitmap bmp)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            for (int i = 0; i < bData.Height; ++i)
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * 3;
                    data[0] = (byte)(255 - data[0]);
                    data[1] = (byte)(255 - data[1]);
                    data[2] = (byte)(255 - data[2]);
                }
            bmp.UnlockBits(bData);
            return new Bitmap(bmp).ToBitmapImage();
        }

        public static Bitmap ToBitmap(this BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadWrite,
                bmp.PixelFormat);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            var bitmapImage = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        public static BitmapImage ToBitmapImage(this BitmapSource source)
        {
            using (var memoryStream = new MemoryStream())
            {
                var encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;
                var bImg = new BitmapImage();
                bImg.BeginInit();
                bImg.StreamSource = memoryStream;
                bImg.EndInit();
                memoryStream.Close();
                return bImg;
            }
        }
        public static BitmapSource ChangeSize(this Bitmap imageBitmap, int height, int width)
        {
            width += imageBitmap.Width;
            height += imageBitmap.Height;
            if (width < 0 || height < 0)
            {
                MessageBox.Show("Введите другие размеры");
                return imageBitmap.ToBitmapImage();
            }
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(imageBitmap, 0, 0, width, height);
            }
            return bitmap.ToBitmapImage();
        }
        public static unsafe BitmapImage ChangeSepia(this Bitmap bmp)
        {
            var bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var scan0 = (byte*)bData.Scan0.ToPointer();
            for (var i = 0; i < bData.Height; ++i)
                for (var j = 0; j < bData.Width; ++j)
                {
                    var data = scan0 + i * bData.Stride + j * 3;
                    var tone = (byte)(0.299 * (*(data + 2)) + 0.587 * (*(data + 1)) + 0.114 * (*data));
                    *(data + 2) = (byte)((tone > 206) ? 255 : tone + 49);
                    *(data + 1) = (byte)((tone < 14) ? 0 : tone - 14);
                    *(data + 0) = (byte)((tone < 56) ? 0 : tone - 56);
                }
            bmp.UnlockBits(bData);
            return new Bitmap(bmp).ToBitmapImage();
        }
        public static unsafe BitmapImage ChangeBrightness(this BitmapSource source, int newBrightness)
        {
            var bmp = source.ToBitmap();
            var bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var scan0 = (byte*)bData.Scan0.ToPointer();
            for (var i = 0; i < bData.Height; ++i)
                for (var j = 0; j < bData.Width; ++j)
                {
                    var data = scan0 + i * bData.Stride + j * 3;
                    *data = (byte)ClampColor(*data + newBrightness);
                    *(data + 1) = (byte)ClampColor(*(data + 1) + newBrightness);
                    *(data + 2) = (byte)ClampColor(*(data + 2) + newBrightness);
                }
            bmp.UnlockBits(bData);
            return bmp.ToBitmapImage();
        }
        public static unsafe BitmapImage ChangeContrast(this BitmapSource source, int contrastValue)
        {
            var bmp = source.ToBitmap();
            if (contrastValue < -100 || contrastValue > 100) return source.ToBitmapImage();
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var scan0 = (byte*)bmpData.Scan0.ToPointer();
            var contrast = (100.0 + contrastValue) / 100.0;
            contrast *= contrast;
            for (var i = 0; i < bmpData.Height; ++i)
                for (var j = 0; j < bmpData.Width; ++j)
                {
                    var data = scan0 + i * bmpData.Stride + j * 3;
                    data[0] = (byte)ClampColor(ChangePixel(data[0], contrast));
                    data[1] = (byte)ClampColor(ChangePixel(data[1], contrast));
                    data[2] = (byte)ClampColor(ChangePixel(data[2], contrast));
                }
            bmp.UnlockBits(bmpData);
            return bmp.ToBitmapImage();
        }
        public static unsafe BitmapImage ChangeColorBalance(this BitmapSource source, int red, int green, int blue)
        {
            var bmp = source.ToBitmap();
            var bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            var scan0 = (byte*)bData.Scan0.ToPointer();
            for (var i = 0; i < bData.Height; ++i)
                for (var j = 0; j < bData.Width; ++j)
                {
                    var data = scan0 + i * bData.Stride + j * 3;

                    *data = (byte)ClampColor(*data + blue);
                    *(data + 1) = (byte)ClampColor(*(data + 1) + green);
                    *(data + 2) = (byte)ClampColor(*(data + 2) + red);
                }
            bmp.UnlockBits(bData);
            return bmp.ToBitmapImage();
        }
        public static BitmapSource EncodeText(this Bitmap bPic, string text)
        {
            Steganography.TextToImage(bPic, text);
            return bPic.ToBitmapImage();
        }
        public static string DecodeText(this BitmapSource source)
        {
            var bPic = source.ToBitmap();
            return Steganography.GetTextFromImage(bPic);
        }
        public static unsafe BitmapImage NormalizeIllumination(this Bitmap bmp)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var scan0 = (byte*)bData.Scan0.ToPointer(); //Pointer to first byte of image
            int r = 0, g = 0, b = 0, avg = 0;
            for (int i = 0; i < bData.Height; ++i)
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * 3;
                    r += *(data + 2);
                    g += *(data + 1);
                    b += *data;
                }
            r /= bData.Height * bData.Width;
            g /= bData.Height * bData.Width;
            b /= bData.Height * bData.Width;
            avg = ((r + g + b) / 3);
            for (int i = 0; i < bData.Height; ++i)
                for (int j = 0; j < bData.Width; ++j)
                {
                    byte* data = scan0 + i * bData.Stride + j * 3;
                    *(data + 2) = (byte)(*(data + 2) * avg / r);
                    *(data + 1) = (byte)(*(data + 1) * avg / g);
                    *data = (byte)(*(data) * avg / b);
                }
            bmp.UnlockBits(bData);
            return new Bitmap(bmp).ToBitmapImage();
        }
        public static BitmapImage ApplyBlur(this Bitmap sourceBitmap)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            double factor = 1.0 / 81.0;
            var filterMatrix = new double[,]
                {
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1},
                    { 1, 1, 1, 1, 1, 1, 1, 1, 1}
                };
            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
            byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];
            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            double blue = 0.0;
            double green = 0.0;
            double red = 0.0;
            int filterWidth = filterMatrix.GetLength(1);
            int filterHeight = filterMatrix.GetLength(0);
            int filterOffset = (filterWidth - 1) / 2;
            int calcOffset = 0;
            int byteOffset = 0;
            for (int offsetY = filterOffset; offsetY < sourceBitmap.Height - filterOffset; offsetY++)
            {
                for (int offsetX = filterOffset; offsetX < sourceBitmap.Width - filterOffset; offsetX++)
                {
                    blue = 0;
                    green = 0;
                    red = 0;
                    byteOffset = offsetY * sourceData.Stride + offsetX * 4;

                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + (filterX * 4) + (filterY * sourceData.Stride);

                            blue += (double)(pixelBuffer[calcOffset]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            green += (double)(pixelBuffer[calcOffset + 1]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                            red += (double)(pixelBuffer[calcOffset + 2]) * filterMatrix[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    resultBuffer[byteOffset] = (byte)ClampColor(factor * blue);
                    resultBuffer[byteOffset + 1] = (byte)ClampColor(factor * green);
                    resultBuffer[byteOffset + 2] = (byte)ClampColor(factor * red);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }
            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return new Bitmap(resultBitmap).ToBitmapImage();
        }
        public static BitmapImage Sharpen(this Bitmap image)
        {
            Bitmap sharpenImage = new Bitmap(image.Width, image.Height);
            int filterWidth = 3;
            int filterHeight = 3;
            int w = image.Width;
            int h = image.Height;
            double[,] filter =
            {
                {-1, -1, -1},
                {-1,  9, -1},
                {-1, -1, -1}
            };
            double factor = 1.0;
            for (int x = 0; x < w; ++x)
            {
                for (int y = 0; y < h; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;
                    Color imageColor = image.GetPixel(x, y);
                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + w) % w;
                            int imageY = (y - filterHeight / 2 + filterY + h) % h;
                            imageColor = image.GetPixel(imageX, imageY);
                            red += imageColor.R * filter[filterX, filterY];
                            green += imageColor.G * filter[filterX, filterY];
                            blue += imageColor.B * filter[filterX, filterY];
                        }
                        sharpenImage.SetPixel(x, y, Color.FromArgb(ClampColor(factor * red),
                            ClampColor(factor * green),
                            ClampColor(factor * blue)));
                    }
                }
            }
            return sharpenImage.ToBitmapImage();
        }
    }
}
