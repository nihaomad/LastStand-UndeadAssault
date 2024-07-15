﻿using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fin_TopDownShooter.Properties
{
    internal class Helper
    {
        private static void BlendPictures(Bitmap bg, Bitmap front, int deltaX, int deltaY)
        {
            for (int y = 0; y < front.Height; y++)
            {
                for (int x = 0; x < front.Width; x++)
                {
                    if (front.GetPixel(x, y).A < 255)
                    {
                        Color newColor = bg.GetPixel(x + deltaX, deltaY + deltaY);
                        front.SetPixel(x, y, newColor);
                    }
                }
            }
        }
        public static void BlendPictures(PictureBox back, PictureBox front)
        {
            int leftDifference = Math.Abs(back.Left - front.Left);
            int topDifference = Math.Abs(back.Top - front.Top);

            BlendPictures((Bitmap)back.Image, (Bitmap)front.Image, leftDifference, topDifference);
        }
    }
}
