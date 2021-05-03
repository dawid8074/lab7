using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Open = new OpenFileDialog();

            if (Open.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.ImageLocation = Open.FileName;
                try
                {
                    pictureBox1.Load();
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;

                    modifyHistogram modify = new modifyHistogram(new Bitmap(pictureBox1.Image), new Bitmap(pictureBox1.Image), chart1, chart2);
                    Blur blur = new Blur(new Bitmap(pictureBox1.Image), new Bitmap(pictureBox1.Image), chart1, chart2);
                    pictureBox2.Image = modify.kontrast();
                    pictureBox3.Image = blur.wyrownanie(5);
                    pictureBox4.Image = blur.gauss(5);


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
    }







    class Blur
    {
        Bitmap baseBitmap;
        Chart baseHistogram;
        Chart normalizedHistogram;
        private int[] intensity = new int[3];
        private int[] odchylenie = new int[3];

        public Blur(Bitmap baseBitmap, Bitmap normalizedBitmap, Chart baseHistogram, Chart normalizedHistogram)
        {
            this.baseBitmap = baseBitmap;
            this.baseHistogram = baseHistogram;
            this.normalizedHistogram = normalizedHistogram;
        }


        public Bitmap wyrownanie(int n)
        {
            Color[,] maska = new Color[n, n];
            int[] srednia;
            for (int i = n - 1; i < baseBitmap.Width; i += n)
            {
                for (int j = n - 1; j < baseBitmap.Height; j += n)
                {
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            maska[x, y] = baseBitmap.GetPixel(i - x, j - y);
                        }
                    }
                    srednia = this.srednia(maska);
                    for (int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            baseBitmap.SetPixel(i - x, j - y, Color.FromArgb(srednia[0],
                                srednia[1],
                                srednia[2]));
                        }
                    }
                }
            }
            return baseBitmap;
        }

        private int[] srednia(Color[,] mask)
        {
            int maskSize = mask.Length / mask.GetLength(1);
            int[] srednie = new int[maskSize];
            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    srednie[0] += mask[i, j].R;
                    srednie[1] += mask[i, j].G;
                    srednie[2] += mask[i, j].B;
                }
            }
            srednie[0] /= mask.Length;
            srednie[1] /= mask.Length;
            srednie[2] /= mask.Length;

            return srednie;
        }

        public Bitmap gauss(int radius)
        {
            var size = (radius * 2) + 1;
            var deviation = radius / 2;
            var mask = new double[size, size];
            double sum = 0.0;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    int numerator = -(i * i + j * j);
                    int denominator = 2 * deviation * deviation;
                    var eExpre = Math.Pow(Math.E, numerator / denominator);
                    var value = (eExpre / (2 * Math.PI * deviation * deviation));

                    mask[i + radius, j + radius] = value;
                    sum += value;
                }
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    mask[i, j] /= sum;
                }
            }

            for (int x = radius; x < baseBitmap.Width - radius; x++)
            {
                for (int y = radius; y < baseBitmap.Height - radius; y++)
                {
                    double red = 0, green = 0, blue = 0;

                    for (int i = -radius; i <= radius; i++)
                    {
                        for (int j = -radius; j <= radius; j++)
                        {
                            double temp = mask[i + radius, j + radius];
                            var pixel = baseBitmap.GetPixel(x - i, y - j);

                            red += pixel.R * temp;
                            green += pixel.G * temp;
                            blue += pixel.B * temp;
                        }
                    }
                    baseBitmap.SetPixel(x, y, Color.FromArgb(
                        checkIfInRgb(red), checkIfInRgb(green), checkIfInRgb(blue)));
                }
            }
            return baseBitmap;
        }

        private int checkIfInRgb(double temp)
        {
            if (temp > 255) return 255;
            else if (temp < 0) return 0;
            return (int)temp;
        }
    }
    class modifyHistogram
    {
        Bitmap baseBitmap;
        Bitmap normalizedBitmap;
        int NM;
        private Chart baseHistogram;
        private Chart normalizedHistogram;
        double[] histogramRed = new double[256];
        double[] histogramGreen = new double[256];
        double[] histogramBlue = new double[256];

        public modifyHistogram(Bitmap baseBitmap, Bitmap normalizedBitmap, Chart baseHistogram, Chart normalizedHistogramm)
        {
            this.baseBitmap = baseBitmap;
            this.normalizedBitmap = normalizedBitmap;
            NM = baseBitmap.Width * baseBitmap.Height;
            this.baseHistogram = baseHistogram;
            this.normalizedHistogram = normalizedHistogramm;
            histogramPodstawa();
            fillHistogram();
        }


        private void histogramPodstawa()
        {
            double[] red = new double[256];
            double[] green = new double[256];
            double[] blue = new double[256];
            for (int x = 0; x < baseBitmap.Width; x++)
            {
                for (int y = 0; y < baseBitmap.Height; y++)
                {
                    Color pixel = baseBitmap.GetPixel(x, y);
                    red[pixel.R]++;
                    green[pixel.G]++;
                    blue[pixel.B]++;
                }
            }


            baseHistogram.Series["red"].Points.Clear();
            baseHistogram.Series["green"].Points.Clear();
            baseHistogram.Series["blue"].Points.Clear();
            for (int i = 0; i < 256; i++)
            {
                baseHistogram.Series["red"].Points.AddXY(i, red[i] / NM);
                baseHistogram.Series["green"].Points.AddXY(i, green[i] / NM);
                baseHistogram.Series["blue"].Points.AddXY(i, blue[i] / NM);
            }
            baseHistogram.Invalidate();
        }

        private void fillHistogram()
        {

            for (int i = 0; i < 256; i++)
            {
                histogramRed[i] = kumulacjaHistogramu(i, "red");
                histogramGreen[i] = kumulacjaHistogramu(i, "green");
                histogramBlue[i] = kumulacjaHistogramu(i, "blue");
                normalizedHistogram.Series["red"].Points.AddXY(i, histogramRed[i]);
                normalizedHistogram.Series["green"].Points.AddXY(i, histogramGreen[i]);
                normalizedHistogram.Series["blue"].Points.AddXY(i, histogramBlue[i]);
            }
        }

        private double kumulacjaHistogramu(int poziom, string kolor)
        {
            if (poziom == 0) return baseHistogram.Series[kolor].Points[0].YValues[baseHistogram.Series[kolor].Points[0].YValues.Length - 1];
            else return baseHistogram.Series[kolor].Points[poziom].YValues[baseHistogram.Series[kolor].Points[0].YValues.Length - 1]
                    + kumulacjaHistogramu(poziom - 1, kolor);
        }

        public Bitmap kontrast()
        {

            int r, g, b;
            for (int x = 0; x < baseBitmap.Width; x++)
            {
                for (int y = 0; y < baseBitmap.Height; y++)
                {
                    Color pixel = baseBitmap.GetPixel(x, y);
                    r = (int)(255 * histogramRed[pixel.R]);
                    g = (int)(255 * histogramGreen[pixel.G]);
                    b = (int)(255 * histogramBlue[pixel.B]);
                    normalizedBitmap.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                }
            }
            return normalizedBitmap;
        }
    }
}
