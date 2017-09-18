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
	public class CanvasBackground
	{
		public static Grid GetBackground()
		{
			Grid grid = new Grid();
			grid.Width = 85;
			grid.Height = 65;
			for(int a = 0;a < 6;a++)
			{
				ColumnDefinition cd = new ColumnDefinition();
				cd.Width = new GridLength(1,GridUnitType.Star);
				grid.ColumnDefinitions.Add(cd);
			}
			for(int b = 0;b < 5;b++)
			{
				RowDefinition rd = new RowDefinition();
				rd.Height = new GridLength(1,GridUnitType.Star);
				grid.RowDefinitions.Add(rd);
			}
			for(int i = 0;i < 6;i++)
			{
				for(int j = 0;j < 5;j++)
				{
					if(i == 5 && j == 4)
					{
						CustomBorder cborder = new CustomBorder();
						cborder.LeftBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.RightBorderBrush = new SolidColorBrush(Colors.Red);
						cborder.TopBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.BottomBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.BorderThickness = new Thickness(0,0,2,2);
						grid.Children.Add(cborder);
						Grid.SetColumn(cborder,i);
						Grid.SetRow(cborder,j);
					}
					else if(i == 5)
					{
						CustomBorder cborder = new CustomBorder();
						cborder.LeftBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.RightBorderBrush = new SolidColorBrush(Colors.Red);
						cborder.TopBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.BottomBorderBrush = new SolidColorBrush(Colors.Black);
						cborder.BorderThickness = new Thickness(0,0,2,1);
						grid.Children.Add(cborder);
						Grid.SetColumn(cborder,i);
						Grid.SetRow(cborder,j);
					}
					else if(j == 4)
					{
						Border border = new Border();
						border.BorderBrush = new SolidColorBrush(Colors.Black);
						border.BorderThickness = new Thickness(0,0,1,2);
						grid.Children.Add(border);
						Grid.SetColumn(border,i);
						Grid.SetRow(border,j);
					}
					else
					{
						Border border = new Border();
						border.BorderBrush = new SolidColorBrush(Colors.Black);
						border.BorderThickness = new Thickness(0,0,1,1);
						grid.Children.Add(border);
						Grid.SetColumn(border,i);
						Grid.SetRow(border,j);
					}
				}
			}

			return grid;
		}

		public static Canvas GetCanvas()
		{
			Canvas canvas = new Canvas();
			canvas.Height = 595;
			canvas.Width = 520;

			Grid grid = GetBackground();

			VisualBrush visualbrush = new VisualBrush(grid);

			DrawingBrush drawingBrushAll = new DrawingBrush(
				new GeometryDrawing(
					visualbrush,
					new Pen(new SolidColorBrush(Colors.Gray),double.NaN)
					{
						//DashStyle = dashstyle
					},
					new RectangleGeometry(
						new Rect(0,0,85,65)
						)
					)
				);
			drawingBrushAll.Stretch = Stretch.Fill;
			drawingBrushAll.TileMode = TileMode.Tile;
			drawingBrushAll.Viewbox = new Rect(0,0,85,65);
			drawingBrushAll.ViewboxUnits = BrushMappingMode.Absolute;
			drawingBrushAll.Viewport = new Rect(0,0,85,65);
			drawingBrushAll.ViewportUnits = BrushMappingMode.Absolute;
			canvas.Background = drawingBrushAll;

			Path path = new Path();

			StreamGeometry streamGeomety = new StreamGeometry();
			StreamGeometryContext sgc = streamGeomety.Open();
			sgc.BeginFigure(new Point(0,425),true,true);
			sgc.LineTo(new Point(520,425),true,true);
			sgc.Close();
			path.Data = streamGeomety;
			path.Fill = new SolidColorBrush(Colors.Red);
			path.Stroke = new SolidColorBrush(Colors.Red);
			path.StrokeThickness = 2;
			canvas.Children.Add(path);

			return canvas;
		}
	}
}
