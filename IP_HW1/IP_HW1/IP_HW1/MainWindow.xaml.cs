using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Threading;
using Microsoft.Win32;

namespace IP_HW1
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            gammaInput.Text = "0.3";
            biasInput.Text = "1.0";
            factorInput.Text = "1.2";
        }

        // Gray scale ColorPalette
        ColorPalette GetGrayScalePalette()
        {
            Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            ColorPalette monoPalette = bmp.Palette;

            System.Drawing.Color[] entries = monoPalette.Entries;

            for (int i = 0; i < 256; i++)
            {
                entries[i] = System.Drawing.Color.FromArgb(i, i, i);
            }

            return monoPalette;
        }

        // Message
        public void Message()
        {
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\Output";
            MessageBox.Show("Processing Complete\nOutput Directory : \n" + outputDirectory, "Image Enhancement");

        }
        // Each scale numbers counting
        private int[] PixelCounting(int[,] ImgData, int beginW, int beginH, int w, int h)
        {
            int[] count = new int[256];
            for (int i = 0; i < count.Length; i++)
                count[i] = 0;

            int scale = 0;

            for (int x = beginW; x < w; x++)
            {
                for (int y = beginH; y < h; y++)
                {
                    scale = ImgData[x, y];
                    count[scale]++;
                }
            }
            /*for (int i = 0; i < count.Length; i++)
                Console.WriteLine(i + ":" + count[i]);*/
            return count;
        }

        // CDF computing
        public int[] ComputingCDF(int[] count)
        {
            int sum = 0;
            int[] cdfData = new int[count.Length];
            for (int i = 0; i < cdfData.Length; i++)
            {
                sum += count[i];
                cdfData[i] = sum;
            }
            return cdfData;
        }

        // Get min cdf
        public int GetMinCDF(int[] cdfData)
        {
            int minCdf = 255;
            for (int i = 0; i < cdfData.Length; i++)
            {
                if (cdfData[i] != 0 && cdfData[i] < minCdf)
                    minCdf = cdfData[i];
            }
            return minCdf;
        }

        // Histogram
        public Bitmap DrawingHistogram(int[] count, string filename)
        {
            int histHeight = 168;
            Bitmap img = new Bitmap(256, histHeight);
            System.Drawing.Pen myPan = new System.Drawing.Pen(System.Drawing.Color.DarkGray, 1);
            using (Graphics g = Graphics.FromImage(img))
            {
                int max = 0;
                for (int i = 0; i < count.Length; i++)
                {
                    if (count[i] > max)
                        max = count[i];
                }

                for (int i = 0; i < count.Length; i++)
                {
                    g.DrawLine(Pens.WhiteSmoke,
                        new System.Drawing.Point(i, 0),
                        new System.Drawing.Point(i, img.Height)
                        );
                }

                for (int i = 0; i < count.Length; i++)
                {
                    float pct = (count[i] * histHeight) / max;
                    g.DrawLine(myPan,
                        new System.Drawing.Point(i, img.Height),
                        new System.Drawing.Point(i, img.Height - (int)(pct))
                        );
                }
            }

            if (!Directory.Exists("Output"))
            {
                Directory.CreateDirectory(@"Output\");
            }

            string outputFileName = "Output/" + filename + ".bmp";
            MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Bmp);

            try
            {
                FileStream fs = File.Create(outputFileName);
                byte[] newBitmapData = ms.ToArray();

                fs.Write(newBitmapData, 0, newBitmapData.Length);
                fs.Flush();
                fs.Close();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                Thread.Sleep(1);
                MessageBox.Show("Output Image Failed !!! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
            return img;
        }

        // Histogram Equalization
        private int HistogramEqualization(string path, string filename)
        {
            /* Set output windows clear ###########*/
            Out01.Source = null;
            Out02.Source = null;
            /* Set output windows clear ###########*/


            Bitmap bitmap = new Bitmap(path);
            Bitmap originalHist;
            Bitmap processedHist;
            int w = bitmap.Width;
            int h = bitmap.Height;
            int i;
            int minCdf = 255;
            int[] count = new int[256];
            int[] cdfData = new int[256];

            for (i = 0; i < 256; i++) count[i] = 0;

            int[,] ImgData = new int[bitmap.Width, bitmap.Height];
            int[,] OutputData = new int[bitmap.Width, bitmap.Height];

            /* Get original image pixel data START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    ImgData[x, y] = bitmap.GetPixel(x, y).B;
                }
            }
            /* Get original image pixel data END ################# END */

            /* GLOBAL START ##################################################################### START */

            /* Counting ################# */
            count = PixelCounting(ImgData, 0, 0, w, h);

            /* Original Drawing histogram ################# */
            originalHist = DrawingHistogram(count, filename + "_OriHist");
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  originalHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(originalHist.Width, originalHist.Height));
            OriHist.Source = bs;


            /* CDF ################# */
            cdfData = ComputingCDF(count);

            /* Get min cdf ################# */
            minCdf = GetMinCDF(cdfData);

            /* Mapping START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    double scale = (double)(cdfData[ImgData[x, y]] - minCdf) / (w * h - minCdf) * 255;
                    OutputData[x, y] = (int)Math.Round(scale);
                }
            }
            /* Mapping END ################# END */

            /* Counting ################# */
            count = PixelCounting(OutputData, 0, 0, w, h);

            /* Global Drawing histogram ################# */
            processedHist = DrawingHistogram(count, filename + "_HE_Hist");

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  processedHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(processedHist.Width, processedHist.Height));
            Out02.Source = bs;

            /* Write image START ################# START */
            Bitmap newBitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    int scale = OutputData[x, y];
                    newBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(scale, scale, scale));
                }
            }

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  newBitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(newBitmap.Width, newBitmap.Height));
            Out01.Source = bs;
            /* Write image END ################# END */

            /* Output Global Data START ############################### START*/
            string outputFileName = "Output/" + filename + "_HistogramEqualization.bmp";
            newBitmap.Palette = GetGrayScalePalette();
            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, ImageFormat.Bmp);
            try
            {
                FileStream fs = File.Create(outputFileName);
                byte[] newBitmapData = ms.ToArray();

                fs.Write(newBitmapData, 0, newBitmapData.Length);
                fs.Flush();
                fs.Close();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                Thread.Sleep(1);
                MessageBox.Show("Output Image Failed !!! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                return 1;
            }
            bitmap.Dispose();
            newBitmap.Dispose();
            /* Output Data END ############################### END*/
            return 0;
        }

        //Power Law
        private int PowerLaw(string path, string filename)
        {
            /* Set output windows clear ###########*/
            Out03.Source = null;
            Out04.Source = null;
            /* Set output windows clear ###########*/


            Bitmap bitmap = new Bitmap(path);
            Bitmap originalHist;
            Bitmap processedHist;

            int w = bitmap.Width;
            int h = bitmap.Height;
            int i;
            int[] count = new int[256];
            int[] cdfData = new int[256];
            double gamma = 0;

            for (i = 0; i < 256; i++) count[i] = 0;

            int[,] ImgData = new int[bitmap.Width, bitmap.Height];
            int[,] OutputData = new int[bitmap.Width, bitmap.Height];

            /* Get original image pixel data START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    ImgData[x, y] = bitmap.GetPixel(x, y).B;
                }
            }
            /* Get original image pixel data END ################# END */

            /* START ##################################################################### START */

            /* Counting ################# */
            count = PixelCounting(ImgData, 0, 0, w, h);

            /* Original Drawing histogram ################# */
            originalHist = DrawingHistogram(count, filename + "_OriHist");

            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  originalHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(originalHist.Width, originalHist.Height));
            OriHist.Source = bs;

            /* Get gamma value */
            if (gammaInput.Text == null || string.IsNullOrWhiteSpace(gammaInput.Text))
            {
                MessageBox.Show("Please enter a gamma value\nusing on power law process ! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                return 1;
            }
            else
            {
                gamma = double.Parse(gammaInput.Text);
            }

            /* Mapping START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    double scale = (double)(255 * Math.Pow((double)((double)ImgData[x, y] / 255), gamma));
                    if (scale >= 255) scale = 255;

                    OutputData[x, y] = (int)scale;
                }
            }
            /* Mapping END ################# END */

            /* Counting ################# */
            count = PixelCounting(OutputData, 0, 0, w, h);

            /* Global Drawing histogram ################# */
            processedHist = DrawingHistogram(count, filename + "_PL_Hist");

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  processedHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(processedHist.Width, processedHist.Height));
            Out04.Source = bs;

            /* Write image START ################# START */
            Bitmap newBitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    int scale = OutputData[x, y];
                    newBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(scale, scale, scale));
                }
            }

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  newBitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(newBitmap.Width, newBitmap.Height));
            Out03.Source = bs;
            /* Write image END ################# END */

            /* Output Data START ############################### START*/
            string outputFileName = "Output/" + filename + "_PowerLaw.bmp";
            newBitmap.Palette = GetGrayScalePalette();
            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, ImageFormat.Bmp);
            try
            {
                FileStream fs = File.Create(outputFileName);
                byte[] newBitmapData = ms.ToArray();

                fs.Write(newBitmapData, 0, newBitmapData.Length);
                fs.Flush();
                fs.Close();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                Thread.Sleep(1);
                MessageBox.Show("Output Image Failed !!! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
            bitmap.Dispose();
            newBitmap.Dispose();
            /* Output Data END ############################### END*/

            return 0;
        }

        //LaplacianSharpening
        private int LaplacianSharpening(string path, string filename)
        {
            /* Set output windows clear ###########*/
            Out05.Source = null;
            Out06.Source = null;
            /* Set output windows clear ###########*/


            Bitmap bitmap = new Bitmap(path);
            Bitmap originalHist;
            Bitmap processedHist;

            int w = bitmap.Width;
            int h = bitmap.Height;
            int i;
            int filterWidth = 3;
            int filterHeight = 3;
            int[] count = new int[256];
            int[] cdfData = new int[256];

            for (i = 0; i < 256; i++) count[i] = 0;

            int[,] ImgData = new int[bitmap.Width, bitmap.Height];
            int[,] OutputData = new int[bitmap.Width, bitmap.Height];
            double[,] filter = new double[filterWidth, filterHeight];
            double factor = 1.2;
            double bias = 1.0;
            /* Set Filter */
            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            /* Get original image pixel data START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    ImgData[x, y] = bitmap.GetPixel(x, y).B;
                }
            }
            /* Get original image pixel data END ################# END */

            /* START ##################################################################### START */

            /* Counting ################# */
            count = PixelCounting(ImgData, 0, 0, w, h);

            /* Original Drawing histogram ################# */
            originalHist = DrawingHistogram(count, filename + "_OriHist");

            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  originalHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(originalHist.Width, originalHist.Height));
            OriHist.Source = bs;

            /* Get factor bias value */
            if (factorInput.Text == null || string.IsNullOrWhiteSpace(gammaInput.Text))
            {
                MessageBox.Show("Please enter a factor value\nusing on laplacian sharpening process ! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                return 1;
            }
            else
            {
                factor = double.Parse(factorInput.Text);
            }

            if (biasInput.Text == null || string.IsNullOrWhiteSpace(gammaInput.Text))
            {
                MessageBox.Show("Please enter a factor value\nusing on laplacian sharpening process ! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                return 1;
            }
            else
            {
                bias = double.Parse(biasInput.Text);
            }

            /* Mapping START ################# START */
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    double p = 0.0;
                    double a1 = 0.0, a2 = 0.0, a3 = 0.0, b1 = 0.0, b3 = 0.0, c1 = 0.0, c2 = 0.0, c3 = 0.0;

                    if (x == 0 || x == (w-1) || y == 0 || y == (h-1))
                    {
                        if (x == 0)
                            a1 = b1 = c1 = 0.0;
                        if (x == w)
                            a3 = b3 = c3 = 0.0;
                        if (y == 0)
                            a1 = b1 = c1 = 0.0;
                        if (y == h)
                            c1 = c2 = c3 = 0.0;
                    }

                    else
                    {
                        a1 = ImgData[x - 1, y - 1];
                        a2 = ImgData[x - 1, y];
                        a3 = ImgData[x - 1, y + 1];
                        b1 = ImgData[x, y - 1];
                        b3 = ImgData[x, y + 1];
                        c1 = ImgData[x + 1, y - 1];
                        c2 = ImgData[x + 1, y];
                        c3 = ImgData[x + 1, y + 1];
                    }

                    p = a1 * filter[0,0] + a2 * filter[0,1] + a3 * filter[0, 2] +
                         b1 * filter[1,0] + ImgData[x, y] * filter[1,1] + b3 * filter[1, 2] +
                         c1 * filter[2,0] + c2 * filter[2,1] + c3 * filter[2, 2];

                    int scale = Math.Min(Math.Max((int)(factor * p + bias), 0), 255);
                    OutputData[x, y] = scale;
                }
            }
            /* Mapping END ################# END */

            /* Counting ################# */
            count = PixelCounting(OutputData, 0, 0, w, h);

            /* Global Drawing histogram ################# */
            processedHist = DrawingHistogram(count, filename + "_LS_Hist");

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  processedHist.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(processedHist.Width, processedHist.Height));
            Out06.Source = bs;

            /* Write image START ################# START */
            Bitmap newBitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            for (int x = 0; x < h; x++)
            {
                for (int y = 0; y < w; y++)
                {
                    int scale = OutputData[x, y];
                    newBitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(scale, scale, scale));
                }
            }

            bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  newBitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(newBitmap.Width, newBitmap.Height));
            Out05.Source = bs;
            /* Write image END ################# END */

            /* Output Data START ############################### START*/
            string outputFileName = "Output/" + filename + "_LaplacianSharpening.bmp";
            newBitmap.Palette = GetGrayScalePalette();
            MemoryStream ms = new MemoryStream();
            newBitmap.Save(ms, ImageFormat.Bmp);
            try
            {
                FileStream fs = File.Create(outputFileName);
                byte[] newBitmapData = ms.ToArray();

                fs.Write(newBitmapData, 0, newBitmapData.Length);
                fs.Flush();
                fs.Close();
                ms.Flush();
                ms.Close();
            }
            catch (Exception ex)
            {
                Thread.Sleep(1);
                MessageBox.Show("Output Image Failed !!! ", "Image Enhancement", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
            bitmap.Dispose();
            newBitmap.Dispose();
            /* Output Data END ############################### END*/
            return 0;
        }

        public void CheckInputDir()
        {
            if (!Directory.Exists("Image"))
            {
                MessageBox.Show("Input Directory \"Image\" does not exist,\nplease create a directory \"Image\" " +
                    "and make sure the three default images are under the directory.", "Image Enhancement");
                System.Environment.Exit(System.Environment.ExitCode);
            }
        }

        private void Btn01_Click(object sender, RoutedEventArgs e)
        {
            CheckInputDir();
            int flag = 0;
            Bitmap bitmap = new Bitmap("Image/Cameraman.bmp");
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  bitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
            OriImage.Source = bs;

            flag += HistogramEqualization("Image/Cameraman.bmp", "Cameraman");
            flag += PowerLaw("Image/Cameraman.bmp", "Cameraman");
            flag += LaplacianSharpening("Image/Cameraman.bmp", "Cameraman");
            if (flag == 0) Message();
        }

        private void Btn02_Click(object sender, RoutedEventArgs e)
        {
            CheckInputDir();
            int flag = 0;
            Bitmap bitmap = new Bitmap("Image/Lena.bmp");
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  bitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
            OriImage.Source = bs;
            flag += HistogramEqualization("Image/Lena.bmp", "Lena");
            flag += PowerLaw("Image/Lena.bmp", "Lena");
            flag += LaplacianSharpening("Image/Lena.bmp", "Lena");
            if (flag == 0) Message();
        }

        private void Btn03_Click(object sender, RoutedEventArgs e)
        {
            CheckInputDir();
            int flag = 0;
            Bitmap bitmap = new Bitmap("Image/Peppers.bmp");
            BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                  bitmap.GetHbitmap(),
                  IntPtr.Zero,
                  System.Windows.Int32Rect.Empty,
                  BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
            OriImage.Source = bs;
            flag += HistogramEqualization("Image/Peppers.bmp", "Peppers");
            flag += PowerLaw("Image/Peppers.bmp", "Peppers");
            flag += LaplacianSharpening("Image/Peppers.bmp", "Peppers");
            if (flag == 0) Message();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            CheckInputDir();
            string pathFilename = null;
            string filename = null;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "圖像文件(JPeg,Gif,Bmp,etc.)|*.jpg;*.jpeg;*.gif;*.bmp;*.tif;*.tiff;*.png|所有文件(*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == false)
            {
                return;
            }
            else
            {
                pathFilename = dlg.FileName;
                filename = System.IO.Path.GetFileName(pathFilename);

                string[] words = filename.Split('.');
                string fname = words[0];

                int flag = 0;
                Bitmap bitmap = new Bitmap(pathFilename);
                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                      bitmap.GetHbitmap(),
                      IntPtr.Zero,
                      System.Windows.Int32Rect.Empty,
                      BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height));
                OriImage.Source = bs;
                flag += HistogramEqualization(pathFilename, fname);
                flag += PowerLaw(pathFilename, fname);
                flag += LaplacianSharpening(pathFilename, fname);
                if (flag == 0) Message();
            }
            
        }

        private void BtnOpenDir_Click(object sender, RoutedEventArgs e)
        {
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string outputDirectory = currentDirectory + "\\Output";
            MessageBox.Show("Output Directory : \n" + outputDirectory, "Image Enhancement");
        }
    }
}
