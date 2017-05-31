using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kursach.ImageProcessing
{
	public class Steganography
	{
	    public static void TextToImage(Bitmap bmp, string text)
	    {
	        if (text.Length * 4 > bmp.Height * bmp.Width) return;
	        int i_for_string = 0;
	        bmp.SetPixel(0, 0, Color.FromArgb(text.Length));
	        bmp.SetPixel(0, 1, Color.FromArgb(0x169));
	        for (int i = 0; i < bmp.Height; i++)
	        for (int j = 2; j < bmp.Width; j++)
	        {
	            Color c = bmp.GetPixel(i, j);
	            byte r = c.R,
	                g = c.G,
	                b = c.B,
	                a = c.A;
	            char symbol = text[i_for_string++];
	            if ( (symbol & 0xff00) >> 2 != 0)
	            {
	                MessageBox.Show("Cтеганография не должна содеражать русские символы", "Ошибка", MessageBoxButton.OK,
	                    MessageBoxImage.Warning);
	                return;
	            }
	            r &= 0xfc; g &= 0xfc; b &= 0xfc; a &= 0xfc;
	            byte new_r = (byte)((symbol & 0xc0) >> 6),
	                new_g = (byte)((symbol & 0x30) >> 4),
	                new_b = (byte)((symbol & 0xc) >> 2),
	                new_a = (byte)(symbol & 0x3);
	            bmp.SetPixel(i, j, Color.FromArgb(a | new_a, r | new_r, g | new_g, b | new_b));
	            if (i_for_string >= text.Length) return;
	        }
	    }
	    public static string GetTextFromImage(Bitmap bmp)
	    {
	        string rc = "";
	        int length = bmp.GetPixel(0, 0).ToArgb();
	        if (bmp.GetPixel(0, 1).ToArgb() != 0x169) return "Изображение не содержит стеганографии";
	        for (int j = 0; j < bmp.Height; j++)
	        {
	            for (int k = 2; k < bmp.Width; k++)
	            {
	                Color c = bmp.GetPixel(j, k);
	                byte r = (byte)((c.R & 0x3) << 6), g = (byte)((c.G & 0x3) << 4), b = (byte)((c.B & 0x3) << 2), a = (byte)(c.A & 0x3);
	                rc += (char)(r | g | b | a);
	                if (rc.Length == length) return rc;
	            }
	        }
	        return rc;
	    }

    }
}
