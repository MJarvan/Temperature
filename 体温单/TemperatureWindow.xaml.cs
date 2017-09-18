using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 体温单
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class TemperatureWindow:Window
	{
		private DataTable throbdt = new DataTable("throbdt");
		public DataTable Throbdt
		{
			get
			{
				return this.throbdt;
			}
			set
			{
				this.throbdt = value;
			}
		}

		private DataTable temdt = new DataTable("temdt");
		public DataTable Temdt
		{
			get
			{
				return this.temdt;
			}
			set
			{
				this.temdt = value;
			}
		}

		SolidColorBrush blackSB = new SolidColorBrush(Colors.Black);
		SolidColorBrush redSB = new SolidColorBrush(Colors.Red);
		SolidColorBrush transSB = new SolidColorBrush(Colors.Transparent);


		public TemperatureWindow()
		{
			InitializeComponent();
			UseLayoutRounding = true;
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			AddTableHead();
			AddTimePoint();
			AddOther();
			AddData();
			PointTheCanvas();
		}

		private void AddOther()
		{
			for(int i = 0;i < othersGrid.ColumnDefinitions.Count - 1;i++)
			{
				for(int j = 0;j < othersGrid.RowDefinitions.Count;j++)
				{
					int columnCount = i + 1;
					int rowCount = j;
					CustomBorder cborder = new CustomBorder();
					//cborder.Name = "border" + columnCount.ToString() + rowCount.ToString();
					//cborder.ToolTip = "border" + columnCount.ToString() + rowCount.ToString();
					cborder.LeftBorderBrush = blackSB;
					cborder.RightBorderBrush = redSB;
					cborder.TopBorderBrush = blackSB;
					cborder.BottomBorderBrush = blackSB;
					if(rowCount == 0)
					{
						cborder.BorderThickness = new Thickness(0,1,2,1);
						Grid grid = new Grid();
						for(int a = 0;a < 6;a++)
						{
							ColumnDefinition cd = new ColumnDefinition();
							cd.Width = new GridLength(1,GridUnitType.Star);
							grid.ColumnDefinitions.Add(cd);
							Border border = new Border();
							border.BorderBrush = blackSB;
							if(a == 5)
							{
								border.BorderThickness = new Thickness(0);
							}
							else
							{
								border.BorderThickness = new Thickness(0,0,1,0);
							}
							TextBlock textblock = new TextBlock();
							textblock.FontSize = 10;
							textblock.Foreground = new SolidColorBrush(Colors.Red);
							textblock.Text = (20).ToString();
							if(a % 2 == 0)
							{
								textblock.VerticalAlignment = VerticalAlignment.Top;
							}
							else
							{
								textblock.VerticalAlignment = VerticalAlignment.Bottom;
							}
							textblock.HorizontalAlignment = HorizontalAlignment.Center;
							border.Child = textblock;
							grid.Children.Add(border);
							Grid.SetColumn(border,a);
						}
						cborder.Child = grid;
					}
					else if (rowCount == 1)
					{
						cborder.BorderThickness = new Thickness(0,0,2,1);
						Grid grid = new Grid();
						for(int b = 0;b < 2;b++)
						{
							ColumnDefinition cd = new ColumnDefinition();
							cd.Width = new GridLength(1,GridUnitType.Star);
							grid.ColumnDefinitions.Add(cd);
							Border border = new Border();
							border.BorderBrush = blackSB;
							if(b == 1)
							{
								border.BorderThickness = new Thickness(0);
							}
							else
							{
								border.BorderThickness = new Thickness(0,0,1,0);
							}
							TextBlock textblock = new TextBlock();
							textblock.FontSize = 10;
							textblock.Text = "133/88";
							if(b % 2 == 0)
							{
								textblock.VerticalAlignment = VerticalAlignment.Top;
							}
							else
							{
								textblock.VerticalAlignment = VerticalAlignment.Bottom;
							}
							textblock.HorizontalAlignment = HorizontalAlignment.Center;
							border.Child = textblock;
							grid.Children.Add(border);
							Grid.SetColumn(border,b);
						}
						cborder.Child = grid;
					}
					else
					{
						cborder.BorderThickness = new Thickness(0,0,2,1);
						TextBlock textblock = new TextBlock();
						textblock.Name = "textblock" + columnCount.ToString() + rowCount.ToString();
						textblock.FontSize = 10;
						textblock.Text = "1";
						textblock.VerticalAlignment = VerticalAlignment.Top;
						textblock.HorizontalAlignment = HorizontalAlignment.Center;
						cborder.Child = textblock;
					}
					othersGrid.Children.Add(cborder);
					Grid.SetColumn(cborder,columnCount);
					Grid.SetRow(cborder,rowCount);
				}
			}

			
		}

		static Point startpoint;
		static Point endpoint;

		private void PointTheCanvas()
		{
			TemperatureSign ts = new TemperatureSign();

			#region 体温

			for(int i = 0;i < Temdt.Rows.Count;i++)
			{

				Grid grid = ts.GetTemX();
				int idTem = Convert.ToInt32(Temdt.Rows[i]["id"]);
				int timeTem = Convert.ToInt32(Temdt.Rows[i]["time"]);
				double tem = Convert.ToDouble(Temdt.Rows[i]["temperature"]);
				//double test = myCanvas.ActualWidth;
				grid.ToolTip = idTem.ToString() + " " + timeTem.ToString() + " " + tem.ToString();
				myCanvas.Children.Add(grid);
				double leftTem = idTem * 85 + (timeTem - 3) / 4 * (85 / 6) + 2;
				double topTem = (42 - tem) * 64;
				grid.SetValue(Canvas.LeftProperty,leftTem);
				grid.SetValue(Canvas.TopProperty,topTem);
				//XX偏移量是4
				if(i == 0)
				{
					startpoint = new Point(leftTem + 4,topTem + 4);
				}
				else
				{
					endpoint = new Point(leftTem + 4,topTem + 4);

					Path path = new Path();

					StreamGeometry streamGeomety = new StreamGeometry();
					StreamGeometryContext sgc = streamGeomety.Open();
					sgc.BeginFigure(startpoint,true,true);
					sgc.LineTo(endpoint,true,true);
					sgc.Close();
					path.Data = streamGeomety;
					path.Fill = new SolidColorBrush(Colors.Blue);
					path.Stroke = new SolidColorBrush(Colors.Blue);
					path.StrokeThickness = 2;
					myCanvas.Children.Add(path);

					startpoint = endpoint;
				}
			}
			#endregion

			#region 脉搏
			for(int j = 0;j < Throbdt.Rows.Count;j++)
			{
				Ellipse ellipse = ts.GetEllipse();
				int idThrob = Convert.ToInt32(Throbdt.Rows[j]["id"]);
				int timeThrob = Convert.ToInt32(Throbdt.Rows[j]["time"]);
				int throb = Convert.ToInt32(Throbdt.Rows[j]["throb"]);
				//double test = myCanvas.ActualWidth;
				ellipse.ToolTip = idThrob.ToString() + " " + timeThrob.ToString() + " " + throb.ToString();
				myCanvas.Children.Add(ellipse);
				double leftThrob = idThrob * 85 + (timeThrob - 3) / 4 * (85 / 6) + 2;
				double topThrob = (180 - throb) * 64 / 20;
				ellipse.SetValue(Canvas.LeftProperty,leftThrob);
				ellipse.SetValue(Canvas.TopProperty,topThrob);

				if(j == 0)
				{
					startpoint = new Point(leftThrob + 4,topThrob + 4);
				}
				else
				{
					endpoint = new Point(leftThrob + 4,topThrob + 4);

					Path path = new Path();

					StreamGeometry streamGeomety = new StreamGeometry();
					StreamGeometryContext sgc = streamGeomety.Open();
					sgc.BeginFigure(startpoint,true,true);
					sgc.LineTo(endpoint,true,true);
					sgc.Close();
					path.Data = streamGeomety;
					path.Fill = new SolidColorBrush(Colors.Red);
					path.Stroke = new SolidColorBrush(Colors.Red);
					path.StrokeThickness = 2;
					myCanvas.Children.Add(path);

					startpoint = endpoint;
				}
			}
			#endregion
			
		}

		private void AddData()
		{
			Temdt.Columns.Add("id",typeof(int));
			Temdt.Columns.Add("time",typeof(int));
			Temdt.Columns.Add("temperature",typeof(double));
			int id1 = 0;
			int time1 = 3;
			Random random = new Random();

			for(int i = 0;i < 42;i++)
			{
				double db = NextDouble(random,36.5,37.5);
				db = Math.Round(db,2);
				DataRow dr = Temdt.NewRow();
				dr[0] = id1;
				dr[1] = time1;
				dr[2] = db;
				Temdt.Rows.Add(dr);
				if((i + 1) % 6 == 0)
				{
					id1++;
				}
				time1 = time1 + 4;
				if(time1 == 27)
				{
					time1 = 3;
				}
			}

			//for(int i = 0;i < 7;i++)
			//{
			//	double db = NextDouble(random,36.5,37.5);
			//	db = Math.Round(db,2);
			//	DataRow dr = Temdt.NewRow();
			//	dr[0] = i;
			//	dr[1] = 15;
			//	dr[2] = db;
			//	Temdt.Rows.Add(dr);
			//}

			Throbdt.Columns.Add("id",typeof(int));
			Throbdt.Columns.Add("time",typeof(int));
			Throbdt.Columns.Add("throb",typeof(int));
			int id2 = 0;
			int time2 = 3;

			for(int j = 0;j < 42;j++)
			{
				double db = NextDouble(random,80.0,100.0);
				db = Math.Round(db,0);
				DataRow dr = Throbdt.NewRow();
				dr[0] = id2;
				dr[1] = time2;
				dr[2] = (int)db;
				Throbdt.Rows.Add(dr);
				if((j + 1) % 6 == 0)
				{
					id2++;
				}
				time2 = time2 + 4;
				if(time2 == 27)
				{
					time2 = 3;
				}
			}

			//for(int j = 0;j < 7;j++)
			//{
			//	double db = NextDouble(random,80.0,100.0);
			//	db = Math.Round(db,0);
			//	DataRow dr = Throbdt.NewRow();
			//	dr[0] = j;
			//	dr[1] = 15;
			//	dr[2] = (int)db;
			//	Throbdt.Rows.Add(dr);
			//}
		}

		private void AddTableHead()
		{
			for(int i = 0;i < tableheadGrid.ColumnDefinitions.Count - 1;i++)
			{
				for(int j = 0;j < tableheadGrid.RowDefinitions.Count;j++)
				{
					int columnCount = i + 1;
					int rowCount = j;
					CustomBorder cborder = new CustomBorder();
					//cborder.Name = "border" + columnCount.ToString() + rowCount.ToString();
					//cborder.ToolTip = "border" + columnCount.ToString() + rowCount.ToString();
					cborder.LeftBorderBrush = blackSB;
					cborder.RightBorderBrush = redSB;
					cborder.TopBorderBrush = blackSB;
					cborder.BottomBorderBrush = blackSB;
					if(rowCount == 1)
					{
						cborder.BorderThickness = new Thickness(0,0,2,0);
						TextBlock textblock = new TextBlock();
						textblock.Name = "textblock" + columnCount.ToString() + rowCount.ToString();
						//textblock.Text = "textblock" + columnCount.ToString() + rowCount.ToString();
						textblock.VerticalAlignment = VerticalAlignment.Center;
						textblock.HorizontalAlignment = HorizontalAlignment.Center;
						cborder.Child = textblock;
					}
					else if(rowCount == 0)
					{
						cborder.BorderThickness = new Thickness(0,1,2,1);

						Button button = new Button();
						button.Name = "datebutton" + columnCount.ToString();
						button.ToolTip = DateTime.Today.AddDays(columnCount).ToShortDateString();
						button.Content = DateTime.Today.AddDays(columnCount).ToShortDateString();
						button.Click += HeaderButtonClick;
						button.Background = new SolidColorBrush(Colors.White);
						button.BorderThickness = new Thickness(0);
						cborder.Child = button;
					}
					else
					{
						cborder.BorderThickness = new Thickness(0,1,2,1);
						TextBlock textblock = new TextBlock();
						textblock.Name = "textblock" + columnCount.ToString() + rowCount.ToString();
						//textblock.Text = "textblock" + columnCount.ToString() + rowCount.ToString();
						textblock.VerticalAlignment = VerticalAlignment.Center;
						textblock.HorizontalAlignment = HorizontalAlignment.Center;
						cborder.Child = textblock;
					}
					tableheadGrid.Children.Add(cborder);
					Grid.SetColumn(cborder,columnCount);
					Grid.SetRow(cborder,rowCount);
				}
			}
			
		}

		//日期缓存变色
		static string dateButtonName = string.Empty;

		private void HeaderButtonClick(object sender,RoutedEventArgs e)
		{
			Button redBt = sender as Button;

			if(dateButtonName == string.Empty)
			{
				dateButtonName = redBt.Name;
				redBt.Foreground = new SolidColorBrush(Colors.Red);
			}
			else if(dateButtonName == redBt.Name)
			{
				return;
			}
			else
			{
				redBt.Foreground = redSB;
				Button blackheadBt = GetChildObject<Button>(tableheadGrid,dateButtonName);
				blackheadBt.Foreground = blackSB;
				Button blackBt = GetChildObject<Button>(timepointGrid,timeButtonName);
				blackBt.Foreground = blackSB;
				dateButtonName = redBt.Name;
				timeButtonName = string.Empty;
			}
		}

		private void AddTimePoint()
		{
			for(int i = 0;i < timepointGrid.ColumnDefinitions.Count - 1;i++)
			{
				int columnCount = i + 1;
				CustomBorder cborder = new CustomBorder();
				cborder.LeftBorderBrush = blackSB;
				cborder.RightBorderBrush = redSB;
				cborder.TopBorderBrush = blackSB;
				cborder.BottomBorderBrush = blackSB;
				cborder.BorderThickness = new Thickness(0,0,2,1);
				Grid grid = new Grid();
				for(int j = 0;j < 6;j++)
				{
					ColumnDefinition cd = new ColumnDefinition();
					cd.Width = new GridLength(1,GridUnitType.Star);
					grid.ColumnDefinitions.Add(cd);
					Border border = new Border();
					border.BorderBrush = blackSB;
					if(j == 5)
					{
						border.BorderThickness = new Thickness(0);
					}
					else
					{
						border.BorderThickness = new Thickness(0,0,1,0);
					}
					Button button = new Button();
					button.FontSize = 9;
					button.Content = (j + 3 * (j + 1)).ToString();
					button.Name = "timebutton" + columnCount.ToString() + j.ToString();
					button.Tag = columnCount;
					button.BorderThickness = new Thickness(0);
					button.Background = transSB;
					button.Click += TimePointButtonClick; 
					border.Child = button;
					grid.Children.Add(border);
					Grid.SetColumn(border,j);
				}
				cborder.Child = grid;
				timepointGrid.Children.Add(cborder);
				Grid.SetColumn(cborder,columnCount);
				Grid.SetRow(cborder,0);
			}
		}

		//时间点缓存变色
		static string timeButtonName = string.Empty;

		private void TimePointButtonClick(object sender,RoutedEventArgs e)
		{
			if(dateButtonName == string.Empty)
			{
				return;
			}

			Button redBt = sender as Button;

			string fatherName = "datebutton" + redBt.Tag.ToString();

			Button father = GetChildObject<Button>(tableheadGrid,fatherName);

			if(father.Foreground.ToString() == blackSB.ToString())
			{
				return;
			}
			else
			{
				if(timeButtonName == string.Empty)
				{
					timeButtonName = redBt.Name;
					redBt.Foreground = new SolidColorBrush(Colors.Red);
				}
				else if(timeButtonName == redBt.Name)
				{
					return;
				}
				else
				{
					redBt.Foreground = redSB;
					Button blackBt = GetChildObject<Button>(timepointGrid,timeButtonName);
					blackBt.Foreground = blackSB;
					timeButtonName = redBt.Name;
				}
			}
		}

		/// <summary>
		/// 生成设置范围内的Double的随机数
		/// eg:_random.NextDouble(1.5, 2.5)
		/// </summary>
		/// <param name="random">Random</param>
		/// <param name="miniDouble">生成随机数的最小值</param>
		/// <param name="maxiDouble">生成随机数的最大值</param>
		/// <returns>当Random等于NULL的时候返回0;</returns>
		public static double NextDouble(Random random,double miniDouble,double maxiDouble)
		{
			if(random != null)
			{
				return random.NextDouble() * (maxiDouble - miniDouble) + miniDouble;
			}
			else
			{
				return 0.0d;
			}
		}

		public T GetChildObject<T>(DependencyObject obj,string name) where T : FrameworkElement
		{
			DependencyObject child = null;
			T grandChild = null;
			for(int i = 0;i <= VisualTreeHelper.GetChildrenCount(obj) - 1;i++)
			{
				child = VisualTreeHelper.GetChild(obj,i);
				if(child is T && (((T)child).Name == name || string.IsNullOrEmpty(name)))
				{
					return (T)child;
				}
				else
				{
					grandChild = GetChildObject<T>(child,name);
					if(grandChild != null)
						return grandChild;
				}
			}
			return null;
		}
	}
	public class MyCanvas:Canvas
	{
		public MyCanvas()
		{
			Grid grid = CanvasBackground.GetBackground();

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
			this.Background = drawingBrushAll;

			Path path = new Path();

			StreamGeometry streamGeomety = new StreamGeometry();
			StreamGeometryContext sgc = streamGeomety.Open();
			sgc.BeginFigure(new Point(0,324),true,true);
			sgc.LineTo(new Point(594,324),true,true);
			sgc.Close();
			path.Data = streamGeomety;
			path.Fill = new SolidColorBrush(Colors.Red);
			path.Stroke = new SolidColorBrush(Colors.Red);
			path.StrokeThickness = 2;
			this.Children.Add(path);
			//SetLeft(path,0);
			//SetTop(path,0);
		}
	}
}