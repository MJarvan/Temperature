using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace 体温单
{
	public class TemperatureSign
	{
		public Ellipse GetEllipse()
		{
			Ellipse ellipse = new Ellipse();
			ellipse.Fill = new SolidColorBrush(Colors.Red);
			ellipse.StrokeThickness = 0;
			ellipse.Width = 8;
			ellipse.Height = 8;
			return ellipse;
		}

		public Grid GetTemX()
		{
			Grid grid = new Grid();
			grid.Width = 8;
			grid.Height = 8;

			LinearGradientBrush linearBrush = new LinearGradientBrush();

			linearBrush.StartPoint = new Point(0,1);
			linearBrush.EndPoint = new Point(1,0);
			linearBrush.GradientStops.Add(new GradientStop(Colors.Transparent,0.0));
			linearBrush.GradientStops.Add(new GradientStop(Colors.Transparent,0.3));
			linearBrush.GradientStops.Add(new GradientStop(Colors.Blue,0.3));
			linearBrush.GradientStops.Add(new GradientStop(Colors.Blue,0.7));
			linearBrush.GradientStops.Add(new GradientStop(Colors.Transparent,0.7));
			linearBrush.GradientStops.Add(new GradientStop(Colors.Transparent,1.0));


			DrawingBrush drawingBrush = new DrawingBrush(
				new GeometryDrawing(
					linearBrush,
					new Pen(new SolidColorBrush(Colors.Gray),double.NaN)
					{
						//DashStyle = dashstyle
					},
					new RectangleGeometry(
						new Rect(0,0,4,4)
						)
					)
				);
			drawingBrush.Stretch = Stretch.Fill;
			drawingBrush.TileMode = TileMode.FlipXY;
			drawingBrush.Viewbox = new Rect(0,0,4,4);
			drawingBrush.ViewboxUnits = BrushMappingMode.Absolute;
			drawingBrush.Viewport = new Rect(0,0,4,4);
			drawingBrush.ViewportUnits = BrushMappingMode.Absolute;
			grid.Background = drawingBrush;
			return grid;
		}
	}
}
