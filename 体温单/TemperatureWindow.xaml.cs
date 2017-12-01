using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
		#region 属性
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

		//画刷
		SolidColorBrush blackSB = new SolidColorBrush(Colors.Black);
		SolidColorBrush redSB = new SolidColorBrush(Colors.Red);
		SolidColorBrush transSB = new SolidColorBrush(Colors.Transparent);

		//描绘路线用点
		static Point startpoint;
		static Point endpoint;

		//时间点缓存变色
		static string timeButtonName = string.Empty;
		//日期缓存变色
		static string dateButtonName = string.Empty;
		#endregion

		public TemperatureWindow()
		{
			InitializeComponent();
			UseLayoutRounding = true;
		}

		private void Window_Loaded(object sender,RoutedEventArgs e)
		{
			AddData();
			AddTableHead();
			AddTimePoint();
			AddOther();
			PointTheCanvas();
			AddThumb();
		}

		private void AddThumb()
		{
			TemperatureSign ts = new TemperatureSign();
			Thumb thumb = new Thumb();
			thumb.Width = 15;
			thumb.Height = 15;
			thumb.Style = FindResource("thumbStyle") as Style;
			thumb.Name = "thumb";
			thumb.SnapsToDevicePixels = true;
			thumb.DragDelta += Thumb_DragDelta;
			thumb.DragCompleted += Thumb_DragCompleted;
			thumb.Foreground = new SolidColorBrush(Colors.Transparent);
			myCanvas.Children.Add(thumb);
			thumb.Visibility = Visibility.Collapsed;
		}

		private void Thumb_DragCompleted(object sender,DragCompletedEventArgs e)
		{
			Thumb myThumb = (Thumb)sender;
			string ellipsename = myThumb.Tag.ToString();
			Ellipse ellipse = GetChildObject<Ellipse>(myCanvas,ellipsename);
			//ellipse.Visibility = Visibility.Visible;
			//double topThrob = ((180 - Convert.ToDouble(tb.Text)) * 64 - (Convert.ToInt32(tb.Text) - 80)) / 20;

			int throb = (int)(11600 - 20 * (Canvas.GetTop(myThumb) + 4)) / 65;
			ThrobTextBox.IsReadOnly = false;
			ThrobTextBox.Text = throb.ToString();
			ellipse.Tag = throb;
			StackPanel sp = ellipse.ToolTip as StackPanel;
			TextBlock textblock = GetChildObject<TextBlock>(sp,"throbText");
			textblock.Text = "脉搏：" + throb.ToString();
			ellipse.SetValue(Canvas.LeftProperty,Canvas.GetLeft(myThumb) + 4);
			ellipse.SetValue(Canvas.TopProperty,Canvas.GetTop(myThumb) + 4);
			myThumb.Visibility = Visibility.Collapsed;

			Point center = new Point(Canvas.GetLeft(ellipse) + 4,Canvas.GetTop(ellipse) + 4);

			string leftpathname = "path" + ellipsename;
			Path leftpath = GetChildObject<Path>(myCanvas,leftpathname);
			if(leftpath != null)
			{
				PathGeometry pathgeometry = leftpath.Data as PathGeometry;
				PathFigure figure = pathgeometry.Figures[0];
				Point left = figure.StartPoint;
				myCanvas.Children.Remove(leftpath);
				PointNewPoint(left,center,leftpathname);
			}

			int notem = (int)ellipse.DataContext;
			DataRow dr = Throbdt.Rows.Find(notem + 1);
			if(dr != null)
			{
				string rightgridname = "throbEp" + (Convert.ToInt32(dr["id"]) + 1).ToString() + ((Convert.ToInt32(dr["time"]) - 3) / 4).ToString();
				Ellipse rightEp = GetChildObject<Ellipse>(myCanvas,rightgridname);
				Point right = new Point(Canvas.GetLeft(rightEp) + 4,Canvas.GetTop(rightEp) + 4);
				Path rightpath = GetChildObject<Path>(myCanvas,"path" + rightgridname);
				myCanvas.Children.Remove(rightpath);
				PointNewPoint(center,right,"path" + rightgridname);
			}
		}

		private void Thumb_DragDelta(object sender,DragDeltaEventArgs e)
		{
			Thumb myThumb = (Thumb)sender;
			double nTop = Canvas.GetTop(myThumb) + e.VerticalChange;
			double nLeft = Canvas.GetLeft(myThumb) + e.HorizontalChange;
			//防止Thumb控件被拖出容器。  
			if(nTop <= 0)
				nTop = 0;
			if(nTop >= (myCanvas.ActualHeight - myThumb.Height))
				nTop = myCanvas.ActualHeight - myThumb.Height;
			if(nLeft <= 0)
				nLeft = 0;
			if(nLeft >= (myCanvas.ActualWidth - myThumb.Width))
				nLeft = myCanvas.ActualWidth - myThumb.Width;
			Canvas.SetTop(myThumb,nTop);
			//Canvas.SetLeft(myThumb,nLeft);
			BringToFront(myThumb);
		}

		public void BringToFront(UIElement element)//图片置于最顶层显示
		{
			if(element == null)
				return;

			var maxZ = myCanvas.Children.OfType<UIElement>()//linq语句，取Zindex的最大值
			  .Where(x => x != element)
			  .Select(x => Canvas.GetZIndex(x))
			  .Max();
			Canvas.SetZIndex(element,maxZ + 1);
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
					cborder.LeftBorderBrush = blackSB;
					cborder.RightBorderBrush = redSB;
					cborder.TopBorderBrush = blackSB;
					cborder.BottomBorderBrush = blackSB;
					//呼吸
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
					//血压
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
					//其他奇怪的项
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

		private void PointTheCanvas()
		{
			#region 按钮变红

			Button datebutton = GetChildObject<Button>(tableheadGrid,"datebutton1");
			datebutton.Foreground = redSB;
			dateButtonName = datebutton.Name;

			Button timebutton = GetChildObject<Button>(timepointGrid,"timebutton10");
			timebutton.Foreground = redSB;
			timeButtonName = timebutton.Name;

			temTextBox.IsReadOnly = true;
			ThrobTextBox.IsReadOnly = true;

			#endregion

			#region 入院第一天

			TextBlock first = new TextBlock();
			first.TextWrapping = TextWrapping.Wrap;
			first.Width = 13;
			first.Foreground = redSB;
			first.Background = new SolidColorBrush(Colors.White);
			first.Text = "入院丨" + "五时十分";
			myCanvas.Children.Add(first);

			#endregion

			TemperatureSign ts = new TemperatureSign();

			#region 体温

			for(int i = 0;i < Temdt.Rows.Count;i++)
			{

				Grid grid = ts.GetTemX();
				int noTem = Convert.ToInt32(Temdt.Rows[i]["no"]);
				int idTem = Convert.ToInt32(Temdt.Rows[i]["id"]);
				int timeTem = Convert.ToInt32(Temdt.Rows[i]["time"]);
				double tem = Convert.ToDouble(Temdt.Rows[i]["temperature"]);
				StackPanel sp = new StackPanel();
				TextBlock tb1 = new TextBlock();
				tb1.FontWeight = FontWeights.Heavy;
				tb1.Text = "日期：" + DateTime.Today.AddDays(idTem + 1).ToShortDateString();
				TextBlock tb2 = new TextBlock();
				tb2.Text = "时间：" + timeTem.ToString();
				TextBlock tb3 = new TextBlock();
				tb3.Text = "体温：" + tem.ToString();
				tb3.Name = "temText";
				sp.Children.Add(tb1);
				sp.Children.Add(tb2);
				sp.Children.Add(tb3);
				string gridname = "temGrid" + (idTem + 1).ToString() + ((timeTem - 3) / 4).ToString();
				grid.Name = gridname;
				grid.DataContext = noTem;
				grid.Tag = tem;
				grid.ToolTip = sp;
				myCanvas.Children.Add(grid);
				double leftTem = idTem * 85 + (timeTem - 3) / 4 * (85 / 6) + 2;
				double topTem = (42 - tem) * 64 - (tem - 37);
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

					PointNewPoint(startpoint,endpoint,"path" + gridname);

					startpoint = endpoint;
				}
			}
			#endregion

			#region 脉搏
			for(int j = 0;j < Throbdt.Rows.Count;j++)
			{
				Ellipse ellipse = ts.GetEllipse();
				int noThrob = Convert.ToInt32(Throbdt.Rows[j]["no"]);
				int idThrob = Convert.ToInt32(Throbdt.Rows[j]["id"]);
				int timeThrob = Convert.ToInt32(Throbdt.Rows[j]["time"]);
				int throb = Convert.ToInt32(Throbdt.Rows[j]["throb"]);
				StackPanel sp = new StackPanel();
				TextBlock tb1 = new TextBlock();
				tb1.FontWeight = FontWeights.Heavy;
				tb1.Text = "日期：" + DateTime.Today.AddDays(idThrob + 1).ToShortDateString();
				TextBlock tb2 = new TextBlock();
				tb2.Text = "时间：" + timeThrob.ToString();
				TextBlock tb3 = new TextBlock();
				tb3.Text = "脉搏：" + throb.ToString();
				tb3.Name = "throbText";
				sp.Children.Add(tb1);
				sp.Children.Add(tb2);
				sp.Children.Add(tb3);
				string throbname = "throbEp" + (idThrob + 1).ToString() + ((timeThrob - 3) / 4).ToString();
				ellipse.Name = throbname;
				ellipse.DataContext = noThrob;
				ellipse.MouseLeftButtonDown += Ellipse_MouseLeftButtonDown;
				ellipse.Tag = throb;
				ellipse.ToolTip = sp;
				myCanvas.Children.Add(ellipse);
				double leftThrob = idThrob * 85 + (timeThrob - 3) / 4 * (85 / 6) + 2;
				double topThrob = ((180 - throb) * 64 - (throb - 80)) / 20;
				ellipse.SetValue(Canvas.LeftProperty,leftThrob);
				ellipse.SetValue(Canvas.TopProperty,topThrob);

				if(j == 0)
				{
					startpoint = new Point(leftThrob + 4,topThrob + 4);
				}
				else
				{
					endpoint = new Point(leftThrob + 4,topThrob + 4);

					PointNewPoint(startpoint,endpoint,"path" + throbname);

					startpoint = endpoint;
				}
			}
			#endregion
			
		}

		private void Ellipse_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
		{
			Ellipse ellipse = sender as Ellipse;
			Thumb thumb = GetChildObject<Thumb>(myCanvas,"thumb");
			thumb.Visibility = Visibility.Visible;
			thumb.SetValue(Canvas.LeftProperty,Canvas.GetLeft(ellipse) - 4);
			thumb.SetValue(Canvas.TopProperty,Canvas.GetTop(ellipse) - 4);
			thumb.Tag = ellipse.Name;
			//ellipse.Visibility = Visibility.Collapsed;
		}

		private void AddData()
		{
			Temdt.Columns.Add("no",typeof(int));
			Temdt.Columns.Add("id",typeof(int));
			Temdt.Columns.Add("time",typeof(int));
			Temdt.Columns.Add("temperature",typeof(double));
			DataColumn[] temcols = new DataColumn[] { Temdt.Columns["no"] };
			Temdt.PrimaryKey = temcols;
			int id1 = 0;
			int time1 = 3;
			Random random = new Random();

			for(int i = 0;i < 42;i++)
			{
				double db = NextDouble(random,36.5,37.5);
				db = Math.Round(db,2);
				DataRow dr = Temdt.NewRow();
				dr[0] = i;
				dr[1] = id1;
				dr[2] = time1;
				dr[3] = db;
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

			Throbdt.Columns.Add("no",typeof(int));
			Throbdt.Columns.Add("id",typeof(int));
			Throbdt.Columns.Add("time",typeof(int));
			Throbdt.Columns.Add("throb",typeof(int));
			DataColumn[] throbcols = new DataColumn[] { Throbdt.Columns["no"] };
			Throbdt.PrimaryKey = throbcols;
			int id2 = 0;
			int time2 = 3;

			for(int j = 0;j < 42;j++)
			{
				double db = NextDouble(random,80.0,100.0);
				db = Math.Round(db,0);
				DataRow dr = Throbdt.NewRow();
				dr[0] = j;
				dr[1] = id2;
				dr[2] = time2;
				dr[3] = (int)db;
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
					cborder.LeftBorderBrush = blackSB;
					cborder.RightBorderBrush = redSB;
					cborder.TopBorderBrush = blackSB;
					cborder.BottomBorderBrush = blackSB;
					//住院日数
					if(rowCount == 1)
					{
						cborder.BorderThickness = new Thickness(0,0,2,0);
						TextBlock textblock = new TextBlock();
						textblock.Name = "textblock" + columnCount.ToString() + rowCount.ToString();

						DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
						dtFormat.ShortDatePattern = "yyyy/MM/dd";
						DateTime dt = Convert.ToDateTime(dateTextblock.Text,dtFormat);
						TimeSpan ts = DateTime.Today - dt;
						int days = ts.Days + columnCount;
						textblock.Text = days.ToString();
						textblock.VerticalAlignment = VerticalAlignment.Center;
						textblock.HorizontalAlignment = HorizontalAlignment.Center;
						cborder.Child = textblock;
					}
					//日期按钮
					else if(rowCount == 0)
					{
						cborder.BorderThickness = new Thickness(0,1,2,1);

						Button button = new Button();
						button.Name = "datebutton" + columnCount.ToString();
						StackPanel sp = new StackPanel();
						TextBlock tb1 = new TextBlock();
						tb1.FontWeight = FontWeights.Heavy;
						tb1.Text = "日期：" + DateTime.Today.AddDays(columnCount).ToShortDateString();
						TextBlock tb2 = new TextBlock();
						tb2.Text = "血压：" + "133/80";
						sp.Children.Add(tb1);
						sp.Children.Add(tb2);
						button.ToolTip = sp;
						button.Content = DateTime.Today.AddDays(columnCount).ToShortDateString();
						button.Click += HeaderButtonClick;
						button.Background = new SolidColorBrush(Colors.White);
						button.BorderThickness = new Thickness(0);
						cborder.Child = button;
					}
					//手术后日期
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

		private void HeaderButtonClick(object sender,RoutedEventArgs e)
		{
			Button redBt = sender as Button;

			if(dateButtonName == string.Empty)
			{
				dateButtonName = redBt.Name;
				redBt.Foreground = redSB;
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

				timeButtonName = "timebutton" + redBt.Name.Remove(0,redBt.Name.Length - 1) + timeButtonName.Remove(0,timeButtonName.Length-1);
				Button newredBt = GetChildObject<Button>(timepointGrid,timeButtonName);
				newredBt.Foreground = redSB;
				dateButtonName = redBt.Name;

				temTextBox.IsReadOnly = false;
				ThrobTextBox.IsReadOnly = false;

				string tem = "temGrid" + timeButtonName.Remove(0,timeButtonName.Length - 2);
				Grid temgrid = GetChildObject<Grid>(myCanvas,tem);
				temTextBox.Tag = tem;
				temTextBox.DataContext = temgrid.DataContext;
				temTextBox.Text = temgrid.Tag.ToString();

				string throb = "throbEp" + timeButtonName.Remove(0,timeButtonName.Length - 2);
				Ellipse throbep = GetChildObject<Ellipse>(myCanvas,throb);
				ThrobTextBox.Tag = throb;
				ThrobTextBox.DataContext = throbep.DataContext;
				ThrobTextBox.Text = throbep.Tag.ToString();
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
					StackPanel sp = new StackPanel();
					TextBlock tb1 = new TextBlock();
					tb1.FontWeight = FontWeights.Heavy;
					tb1.Text = "日期：" + DateTime.Today.AddDays(columnCount).ToShortDateString();
					TextBlock tb2 = new TextBlock();
					tb2.Text = "时间：" + (j + 3 * (j + 1)).ToString();
					sp.Children.Add(tb1);
					sp.Children.Add(tb2);
					button.ToolTip = sp;
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
					redBt.Foreground = redSB;
				}
				else if(timeButtonName == redBt.Name)
				{
					return;
				}
				else
				{
					temTextBox.IsReadOnly = false;
					ThrobTextBox.IsReadOnly = false;

					string tem = "temGrid" + redBt.Name.Remove(0,redBt.Name.Length - 2);
					Grid temgrid = GetChildObject<Grid>(myCanvas,tem);
					temTextBox.Tag = tem;
					temTextBox.DataContext = temgrid.DataContext;
					temTextBox.Text = temgrid.Tag.ToString();

					string throb = "throbEp" + redBt.Name.Remove(0,redBt.Name.Length - 2);
					Ellipse throbep = GetChildObject<Ellipse>(myCanvas,throb);
					ThrobTextBox.Tag = throb;
					ThrobTextBox.DataContext = throbep.DataContext;
					ThrobTextBox.Text = throbep.Tag.ToString();

					redBt.Foreground = redSB;
					Button blackBt = GetChildObject<Button>(timepointGrid,timeButtonName);
					blackBt.Foreground = blackSB;
					timeButtonName = redBt.Name;
				}
			}
		}
		private void Test_KeyDown(object sender,KeyEventArgs e)
		{
			Regex r = new Regex("^[0-9]*$");

			if(e.Key == Key.Enter)   //  if (e.KeyValue == 13) 判断是回车键
			{
				TextBox tb = sender as TextBox;
				if(tb.Name == "temTextBox")
				{

					if(r.IsMatch(tb.Text.ToString().Trim()) == true && 34.00 <= Convert.ToDouble(tb.Text) && Convert.ToDouble(tb.Text) <= 42.00)
					{
						string tem = tb.Tag.ToString();
						Grid temgrid = GetChildObject<Grid>(myCanvas,tem);
						double topTem = (42 - Convert.ToDouble(tb.Text)) * 64 - (Convert.ToDouble(tb.Text) - 37);
						StackPanel sp = temgrid.ToolTip as StackPanel;
						TextBlock textblock = GetChildObject<TextBlock>(sp,"temText");
						textblock.Text = "体温：" + tb.Text.ToString();

						temgrid.SetValue(Canvas.TopProperty,topTem);
						Point center = new Point(Canvas.GetLeft(temgrid) + 4,Canvas.GetTop(temgrid) + 4);

						string leftpathname = "path" + tem;
						Path leftpath = GetChildObject<Path>(myCanvas,leftpathname);
						if(leftpath != null)
						{
							PathGeometry pathgeometry = leftpath.Data as PathGeometry;
							PathFigure figure = pathgeometry.Figures[0];
							Point left = figure.StartPoint;
							myCanvas.Children.Remove(leftpath);
							PointNewPoint(left,center,leftpathname);
						}

						int notem = (int)temgrid.DataContext;
						DataRow dr = Temdt.Rows.Find(notem + 1);
						if(dr != null)
						{
							string rightgridname = "temGrid" + (Convert.ToInt32(dr["id"]) + 1).ToString() + ((Convert.ToInt32(dr["time"]) - 3) / 4).ToString();
							Grid rightgrid = GetChildObject<Grid>(myCanvas,rightgridname);
							Point right = new Point(Canvas.GetLeft(rightgrid) + 4,Canvas.GetTop(rightgrid) + 4);
							Path rightpath = GetChildObject<Path>(myCanvas,"path" + rightgridname);
							myCanvas.Children.Remove(rightpath);
							PointNewPoint(center,right,"path" + rightgridname);
						}
					}
					else
					{
						MessageBox.Show("请输入正确的值!");
						return;
					}
				}
				else if(tb.Name == "ThrobTextBox")
				{
					if(r.IsMatch(tb.Text.ToString().Trim()) == true && 20 <= Convert.ToInt32(tb.Text) && Convert.ToInt32(tb.Text) <= 180)
					{
						string throb = tb.Tag.ToString();
						Ellipse throbep = GetChildObject<Ellipse>(myCanvas,throb);
						double topThrob = ((180 - Convert.ToDouble(tb.Text)) * 64 - (Convert.ToInt32(tb.Text) - 80)) / 20;
						StackPanel sp = throbep.ToolTip as StackPanel;
						TextBlock textblock = GetChildObject<TextBlock>(sp,"throbText");
						textblock.Text = "脉搏：" + tb.Text.ToString();

						throbep.SetValue(Canvas.TopProperty,topThrob);

						Point center = new Point(Canvas.GetLeft(throbep) + 4,Canvas.GetTop(throbep) + 4);

						string leftpathname = "path" + throb;
						Path leftpath = GetChildObject<Path>(myCanvas,leftpathname);
						if(leftpath != null)
						{
							PathGeometry pathgeometry = leftpath.Data as PathGeometry;
							PathFigure figure = pathgeometry.Figures[0];
							Point left = figure.StartPoint;
							myCanvas.Children.Remove(leftpath);
							PointNewPoint(left,center,leftpathname);
						}

						int notem = (int)throbep.DataContext;
						DataRow dr = Throbdt.Rows.Find(notem + 1);
						if(dr != null)
						{
							string rightgridname = "throbEp" + (Convert.ToInt32(dr["id"]) + 1).ToString() + ((Convert.ToInt32(dr["time"]) - 3) / 4).ToString();
							Ellipse rightEp = GetChildObject<Ellipse>(myCanvas,rightgridname);
							Point right = new Point(Canvas.GetLeft(rightEp) + 4,Canvas.GetTop(rightEp) + 4);
							Path rightpath = GetChildObject<Path>(myCanvas,"path" + rightgridname);
							myCanvas.Children.Remove(rightpath);
							PointNewPoint(center,right,"path" + rightgridname);
						}
					}
					else
					{
						MessageBox.Show("请输入正确的值!");
						return;
					}
				}
				else
				{
					return;
				}
			}
		}

		private void PointNewPoint(Point start,Point end,string pathname)
		{
			Path path = new Path();
			PathGeometry pathgeometry = new PathGeometry();
			PathFigure figure = new PathFigure();
			figure.StartPoint = start;
			LineSegment ls = new LineSegment(end,true);
			figure.Segments.Add(ls);
			pathgeometry.Figures.Add(figure);
			path.Data = pathgeometry;
			path.Name = pathname;
			pathname = pathname.Replace("path",string.Empty);
			pathname = pathname.Substring(0,pathname.Length - 2);
			if(pathname == "temGrid")
			{
				path.Fill = new SolidColorBrush(Colors.Blue);
				path.Stroke = new SolidColorBrush(Colors.Blue);
			}
			else if(pathname == "throbEp")
			{
				path.Fill = redSB;
				path.Stroke = redSB;
			}
			path.StrokeThickness = 2;
			myCanvas.Children.Add(path);
		}

		#region 辅助函数

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

		#endregion
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