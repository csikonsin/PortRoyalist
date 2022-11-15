using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace PortRoyalist
{
    public class WareRectangle
    {
        public WareRectangle(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        //public StockRectangle(int x, int y, int width, int height)
        //{
        //    this.X = x;
        //    this.Y = y;
        //    this.Width = width;
        //    this.Height = height;
        //}

        //public List<Point> Points { get; set; }
    
        public int Width { get; set; }
        public int Height { get; set; }
      
        public int X { get; set; }
        public int Y { get; set; }

    
        public Point TopLeft { get; set; }

        public string Ware { get; set; }
        public enCol Col { get; set; }
        public enum enCol
        {
            Undefined = 0,
            Stock = 1,
            Sell = 2,
            Buy = 3
        }

        public Rectangle ToRectangle(int scaleFactor = 1)
        {
            return new Rectangle(this.X * scaleFactor, this.Y * scaleFactor, this.Width * scaleFactor, this.Height * scaleFactor);
        }
    }
}
