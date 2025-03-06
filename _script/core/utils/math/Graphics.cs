using Godot;
using System.Collections;

namespace Bouncerock 
{ 
    
    public static class Graphics
    {
		public class Line
        {
            private double x0, y0, x1, y1;
            private Color foreColor;
            private byte lineStyleMask;
            private int thickness;
            private float globalm;

            public Line(double x0, double y0, double x1, double y1, float valueToSet, byte lineStyleMask, int thickness)
            {
                this.x0 = x0;
                this.y0 = y0;
                this.y1 = y1;
                this.x1 = x1;

                //this.foreColor = color;

                this.lineStyleMask = lineStyleMask;

                this.thickness = thickness;

            }

            private void plot(float[,] floatArray, double x, double y, double c)
            {
                int alpha = (int)(c * 255);
                if (alpha > 255) alpha = 255;
                if (alpha < 0) alpha = 0;
               // Color color = Color.FromArgb(alpha, foreColor);
               /* if (BitmapDrawHelper.checkIfInside((int)x, (int)y, bitmap))
                {
                    bitmap.SetPixel((int)x, (int)y, color);
                }*/

            }

            int ipart(double x) { return (int)x;}

            int round(double x) {return ipart(x+0.5);}
        
            double fpart(double x) 
            {
                if(x<0) return (1-(Mathf.Floor((float)x)));
                return (Mathf.Floor((float)x));
            }
        
            double rfpart(double x) {
                return 1-fpart(x);
            }


            public void draw(float[,] floatArray) 
            {
                bool steep = Mathf.Abs((float)y1-(float)y0)>Mathf.Abs((float)x1-(float)x0);
                double temp;
                if(steep)
                {
                    temp=x0; x0=y0; y0=temp;
                    temp=x1;x1=y1;y1=temp;
                }
                if(x0>x1)
                {
                    temp = x0;x0=x1;x1=temp;
                    temp = y0;y0=y1;y1=temp;
                }

                double dx = x1-x0;
                double dy = y1-y0;
                double gradient = dy/dx;

                double xEnd = round(x0);
                double yEnd = y0+gradient*(xEnd-x0);
                double xGap = rfpart(x0+0.5);
                double xPixel1 = xEnd;
                double yPixel1 = ipart(yEnd);

                if(steep)
                {
                    plot(floatArray, yPixel1,   xPixel1, rfpart(yEnd)*xGap);
                    plot(floatArray, yPixel1+1, xPixel1,  fpart(yEnd)*xGap);
                }
                else
                {
                    plot(floatArray, xPixel1,yPixel1, rfpart(yEnd)*xGap);
                    plot(floatArray, xPixel1, yPixel1+1, fpart(yEnd)*xGap);
                }
                double intery = yEnd+gradient;

                xEnd = round(x1);
                yEnd = y1+gradient*(xEnd-x1);
                xGap = fpart(x1+0.5);
                double xPixel2 = xEnd;
                double yPixel2 = ipart(yEnd);
                if(steep)
                {
                    plot(floatArray, yPixel2,   xPixel2, rfpart(yEnd)*xGap);
                    plot(floatArray, yPixel2+1, xPixel2, fpart(yEnd)*xGap);
                }
                else
                {
                    plot(floatArray, xPixel2, yPixel2, rfpart(yEnd)*xGap);
                    plot(floatArray, xPixel2, yPixel2+1, fpart(yEnd)*xGap);
                }

                if(steep)
                {
                    for(int x=(int)(xPixel1+1);x<=xPixel2-1;x++){
                        plot(floatArray, ipart(intery), x, rfpart(intery));
                        plot(floatArray, ipart(intery)+1, x, fpart(intery));
                        intery+=gradient;
                    }
                }
                else
                {
                    for(int x=(int)(xPixel1+1);x<=xPixel2-1;x++){
                        plot(floatArray, x,ipart(intery), rfpart(intery));
                        plot(floatArray, x, ipart(intery)+1, fpart(intery));
                        intery+=gradient;
                    }
                }
            }
        }

		
		
		
		
		
		
        //Bresenham's Line Algorithm
        public static float[,] DrawLineBresenham(float[,] original, float valuetoSet, int x0, int y0, int x1, int y1)
            {
                bool steep = Mathf.Abs(y1 - y0) > Mathf.Abs(x1 - x0);
                if (steep)
                {
                    int t;
                    t = x0; // swap x0 and y0
                    x0 = y0;
                    y0 = t;
                    t = x1; // swap x1 and y1
                    x1 = y1;
                    y1 = t;
                }
                if (x0 > x1)
                {
                    int t;
                    t = x0; // swap x0 and x1
                    x0 = x1;
                    x1 = t;
                    t = y0; // swap y0 and y1
                    y0 = y1;
                    y1 = t;
                }
                int dx = x1 - x0;
                int dy = Mathf.Abs(y1 - y0);
                int error = dx / 2;
                int ystep = (y0 < y1) ? 1 : -1;
                int y = y0;
                for (int x = x0; x <= x1; x++)
                {
                    if (x <0 ||  y<0||x >= original.GetLength(0) ||y >= original.GetLength(1))
                    {}
                    else
                    {
                        if (steep) { original[y,x] = valuetoSet; }
                        if (!steep) { original[x,y] = valuetoSet; }
                        error = error - dy;
                        if (error < 0)
                        {
                            y += ystep;
                            error += dx;
                        }
                    }
                }
                return original;
            }
			
		static float[,] SetValue(float[,] array, int x, int y, int value)
        {
            if (x <0 || 
            y<0||
            x >= array.GetLength(0) ||
             y >= array.GetLength(1))
            {
                return array;
            }
            array[x,y] = value;
            return array;
        }

    }
}