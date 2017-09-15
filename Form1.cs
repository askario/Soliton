using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.FreeGlut;
using Tao.Platform.Windows;
namespace lab2kg
{
  unsafe public partial class Form1 : Form
  {
  double ScreenW, ScreenH;
  private float devX;
  private float devY;
  private float[,] GrapValuesArray;
  private int elements_count = 0;
  private bool not_calculate = true;
  private int pointPosition = 0;
  float lineX, lineY;
  float Mcoord_X = 0, Mcoord_Y = 0;
  
  double[] A, newA, V, newV, Q, newQ;
  int N;
  double dt;
  double b;
        double aa;
  bool isFPU;
  double eps;
  bool isVerletLike;

  public Form1()
  {
    InitializeComponent();
    AnT.InitializeContexts();

    eps = 0.5 - Math.Pow((double)(2 * Math.Sqrt((double)326) + 36), (double)(1 / 3.0)) / 12.0 + 1 / 6 / Math.Pow((double)(2 * Math.Sqrt((double)326) + 36), (double)(1 / 3.0));
    dt = 0.005;
    N = 1000;
    A = new double[N];
    newA = new double[N];
    Q = new double[N];
    newQ = new double[N];
    V = new double[N];
    newV = new double[N];

    b = 1000;//betta - 
    aa = 1.6;//alpha
    isFPU = true;
    isVerletLike = true;

    for (int i = 0; i < N; i++)
    {
      Q[i] = 0;
      A[i] = 0;
      V[i] = 0;
    }

    float P0 = 1f;
    V[N / 2] = P0;
  }

  private void Form1_Load(object sender, EventArgs e)
  {
    Glut.glutInit();
    Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);
    Gl.glClearColor(255,255, 255, 1);
    Gl.glViewport(0, 0, AnT.Width, AnT.Height);
    Gl.glMatrixMode(Gl.GL_PROJECTION);
    Gl.glLoadIdentity();
    if ((float)AnT.Width <= (float)AnT.Height)
    {
      ScreenW = 30.0;
      ScreenH = 30.0 * (float)AnT.Height / (float)AnT.Width;
      Glu.gluOrtho2D(0.0, ScreenW,0.0, ScreenH);
    }
    else
    {
      ScreenW = 30.0 * (float)AnT.Width / (float)AnT.Height;
      ScreenH = 30.0;
      Glu.gluOrtho2D(-20.0, 30.0 * (float)AnT.Width / (float)AnT.Height, -10.0, 30.0);
    }
    devX = (float)ScreenH / (float)AnT.Width;
    devY = (float)ScreenW / (float)AnT.Height;
    Gl.glMatrixMode(Gl.GL_MODELVIEW);
    PointGrap.Start();
  }  
  private void PointGrap_Tick(object sender, EventArgs e)
  {
      
    if (pointPosition == elements_count - 1)
        pointPosition = 0;
    Draw();
    pointPosition++;
  }
  private void AnT_MouseMove(object sender, MouseEventArgs e)
  {
  }
  void CountAccHarm(double[] q, double[] a)
  {
    for (int i = 1; i < N - 1; i++)
    {
      a[i] = (q[i + 1] - 2 * q[i] + q[i - 1]);
    }

    a[0] = (q[1] - 2 * q[0] + q[N - 1]);
    a[N - 1] = (q[0] - 2 * q[N - 1] + q[N - 2]);
  }

  void CountAccFPU_b (double[] q, double[] a)
  {
    double temp;
    double summ;
    for (int i = 1; i < N - 1; i++)
    {
      summ = 0;
      temp = q[i + 1] - q[i];
      summ += b * temp * temp * temp;
      temp = q[i] - q[i - 1];
      summ -= b * temp * temp * temp;
      summ += (q[i + 1] - 2 * q[i] + q[i - 1]);

      a[i] = summ;
    }

    summ = 0;
    temp = q[1] - q[0];
    summ += b * temp * temp * temp;
    temp = q[0] - q[N - 1];
    summ -= b * temp * temp * temp;
    summ += (q[1] - 2 * q[0] + q[N - 1]);

    a[0] = summ;

    summ = 0;
    temp = q[0] - q[N - 1];
    summ += b * temp * temp * temp;
    temp = q[N - 1] - q[N - 2];
    summ -= b * temp * temp * temp;
    summ += (q[0] - 2 * q[N - 1] + q[N - 2]);

    a[N - 1] = summ;
  }

  void CountAccFPU_a(double[] q, double[] a)
        {
            double temp;
            double summ;
            for (int i = 1; i < N - 1; i++)
            {
                summ = 0;
                temp = q[i + 1] - q[i];
                summ += aa * temp * temp ;
                temp = q[i] - q[i - 1];
                summ -= aa * temp * temp ;
                summ += (q[i + 1] - 2 * q[i] + q[i - 1]);

                a[i] = summ;
            }

            summ = 0;
            temp = q[1] - q[0];
            summ += aa * temp * temp;
            temp = q[0] - q[N - 1];
            summ -= aa * temp * temp;
            summ += (q[1] - 2 * q[0] + q[N - 1]);

            a[0] = summ;

            summ = 0;
            temp = q[0] - q[N - 1];
            summ += aa * temp * temp;
            temp = q[N - 1] - q[N - 2];
            summ -= aa * temp * temp;
            summ += (q[0] - 2 * q[N - 1] + q[N - 2]);

            a[N - 1] = summ;
        }

        void CountStepVerle(bool isFPU)
  {
    double k;
    for (int i = 0; i < N; i++)
    {
      k = V[i] + 0.5 * A[i] * dt;
      newQ[i] = Q[i] + k * dt;
      newV[i] = k;
    }

    if (isFPU)
      CountAccFPU_a(newQ, newA);
    else
      CountAccHarm(newQ, newA);

    for (int i = 0; i < N; i++)
    {
      V[i] = newV[i] + 0.5 * newA[i] * dt;
    }

    double[] temp = newV;
    newV = V;
    V = temp;
  }

  void CountStepVerleLike(bool isFPU)
  {
    double c1 = eps * dt;

    for (int i = 0; i < N; i++)
    {
      newQ[i] = Q[i] + V[i] * c1;
    }

    if (isFPU)
      CountAccFPU_a(newQ, newV);
    else
      CountAccHarm(newQ, newV);

    double c = (1 - 2 * eps) * dt;
    double c2 = dt * 0.5;

    for (int i = 0; i < N; i++)
    {
      newV[i] *= c2;
      newV[i] += V[i];
      Q[i] = newQ[i] + newV[i] * c;
    }

    if (isFPU)
      CountAccFPU_a(Q, V);
    else
      CountAccHarm(Q, V);

    for (int i = 0; i < N; i++)
    {
      V[i] *= c2;
      V[i] += newV[i];
      newQ[i] = Q[i] + V[i] * c1;
    }

    double[] temp = newV;
    newV = V;
    V = temp;
  }

 private void DrawDiagram()
 {

   double[] temp;
   for (int i = 0; i < 100; i++)
   {
     if (isVerletLike)
       CountStepVerleLike(true);
     else
       CountStepVerle(true);
     temp = Q;
     Q = newQ;
     newQ = temp;

     temp = V;
     V = newV;
     newV = temp;

     temp = A;
     A = newA;
     newA = temp;
   }


   Gl.glBegin(Gl.GL_LINE_STRIP);
   
   Gl.glVertex2d(-50,30*V[0]);
   for (int i = 1; i<N; i++)
   {
     Gl.glVertex2d(-50+i/(float)N*95, 30*V[i]);
   }

   Gl.glEnd();
 
 }

    private void Draw()
    {
      Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
      Gl.glLoadIdentity();
      Gl.glColor3f(0, 0, 0 );
      Gl.glPushMatrix();
      Gl.glTranslated(30, 5, 0);
      
      DrawDiagram();

      Gl.glPopMatrix();
      Gl.glFlush();
      AnT.Invalidate();
    }

    private void AnT_Load(object sender, EventArgs e)
    {

    }
  }
}
