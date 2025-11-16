using System.Windows;
using System.Windows.Input;
using System.Windows.Controls; 
using System.Windows.Media;     
using System.Windows.Shapes;
using AVL_DBMS.ViewModel;
using AVL_DBMS.Model.AVL_Tree_Elements;

namespace AVL_DBMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        private const double NodeRadius = 20; // Радіус кола (вузла)
        private const double VerticalSpacing = 80; // Відстань між рівнями дерева
        private const int maxAllowedNodesForVisualization = 100;

        private const double LargeCanvasWidth = 3000;
        private const double LargeCanvasHeight = 1500;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            _viewModel.TreeChanged += OnTreeChanged;
            this.Loaded += (s, e) => OnTreeChanged();
        }

        private void OnTreeChanged()
        {
            
            var root = _viewModel.GetTreeRoot();            
            VisualizationCanvas.Children.Clear();
            int nodeCount = _viewModel.CurrentTreeCount;

            double visibleWidth = TreeScrollViewer.ActualWidth;
            double visibleHeight = TreeScrollViewer.ActualHeight;

            // Запобігаємо малюванню, якщо вікно ще не завантажилося
            if (visibleWidth == 0 || visibleHeight == 0)
            {
                return;
            }

            // До 30 записів (Мале дерево)
            if (root != null && nodeCount > 0 && nodeCount <= 30)
            {               
                VisualizationCanvas.Width = visibleWidth;
                VisualizationCanvas.Height = visibleHeight;

                // Малюємо, використовуючи видиму ширину
                DrawNodeRecursive(root, visibleWidth / 2, 40, visibleWidth / 4);
            }
            // 30-1000 записів (Більше дерево)
            else if (root != null && nodeCount > 30 && nodeCount <= maxAllowedNodesForVisualization)
            {               
                VisualizationCanvas.Width = LargeCanvasWidth;
                VisualizationCanvas.Height = LargeCanvasHeight;

                // Малюємо, використовуючи велику ширину
                DrawNodeRecursive(root, LargeCanvasWidth / 2, 40, LargeCanvasWidth / 4);
            }
            // Записів > 100
            else if (nodeCount > maxAllowedNodesForVisualization)
            {                
                VisualizationCanvas.Width = visibleWidth;
                VisualizationCanvas.Height = visibleHeight;
                DrawWatermark($"Відображення дерева вимкнено");
            }            
            else
            {
                // Canvas залишається чистим, бо дерево порожнє
                VisualizationCanvas.Width = visibleWidth;
                VisualizationCanvas.Height = visibleHeight;
            }
        }
        private void DrawWatermark(string message)
        {
            TextBlock watermark = new TextBlock
            {
                Text = message,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gray,
                Opacity = 0.5,
                TextWrapping = TextWrapping.Wrap // Для перенесення тексту
            };

            double canvasWidth = VisualizationCanvas.Width;
            double canvasHeight = VisualizationCanvas.Height;

            watermark.Measure(new Size(canvasWidth - 20, double.PositiveInfinity));
            Size textSize = watermark.DesiredSize;

            Canvas.SetLeft(watermark, (canvasWidth - textSize.Width) / 2);
            Canvas.SetTop(watermark, (canvasHeight - textSize.Height) / 2);

            VisualizationCanvas.Children.Add(watermark);
        }

        /// <summary>
        /// Рекурсивний метод малювання дерева.
        /// </summary>
        /// <param name="node">Поточний вузол для малювання.</param>
        /// <param name="x">Центральна X-координата для цього вузла.</param>
        /// <param name="y">Центральна Y-координата для цього вузла.</param>
        /// <param name="horizontalOffset">Горизонтальний зсув для нащадків.</param>
        private void DrawNodeRecursive(AVLNode node, double x, double y, double horizontalOffset)
        {
            if (node == null)
            {
                return; 
            }            
            // Малюємо лінії під вузлами, щоб кола були "зверху"
            double childY = y + VerticalSpacing; // Y-координата для обох нащадків
            double currentOffset = horizontalOffset;
            if (currentOffset < NodeRadius)
            {                
                currentOffset = NodeRadius;
            }

            double nextLevelOffset = horizontalOffset / 2;

            // Лівий нащадок
            if (node.Left != null)
            {
                double childX = x - horizontalOffset;
                // Малюємо лінію від поточного вузла до лівого нащадка
                DrawLine(x, y, childX, childY);
                // Рекурсивно малюємо ліве піддерево
                DrawNodeRecursive(node.Left, childX, childY, nextLevelOffset);
            }

            // Правий нащадок 
            if (node.Right != null)
            {
                double childX = x + horizontalOffset;
                // Малюємо лінію від поточного вузла до правого нащадка
                DrawLine(x, y, childX, childY);
                // Рекурсивно малюємо праве піддерево
                DrawNodeRecursive(node.Right, childX, childY, nextLevelOffset);
            }             
            // Малюємо вузол "поверх" ліній
            DrawNodeVisual(node.Key.ToString(), x, y);
        }

        /// <summary>
        /// Допоміжний метод, що малює одне коло (вузол) і текст на ньому.
        /// </summary>
        private void DrawNodeVisual(string text, double x, double y)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.SkyBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, x - NodeRadius);
            Canvas.SetTop(ellipse, y - NodeRadius);

            // --- Логіка обрізання довгого тексту ---
            string displayText = text;
            int fontSize = 14; // Стандартний розмір

            // Встановлюємо ліміт довжини для одного рядка
            const int lineLengthLimit = 3;

            if (text.Length > lineLengthLimit)
            {
                // Зменшуємо шрифт, щоб вмістити два рядки
                fontSize = 10;

                if (text.Length > lineLengthLimit * 2) // Якщо довше 6 символів
                {
                    // Обрізаємо і додаємо \n та ...
                    // "1234567" -> "123\n456..."
                    displayText = text.Substring(0, lineLengthLimit) +
                                  "\n" +
                                  text.Substring(lineLengthLimit, lineLengthLimit) +
                                  "...";
                }
                else // Якщо від 4 до 6 символів
                {
                    // "12345" -> "123\n45"
                    displayText = text.Substring(0, lineLengthLimit) +
                                  "\n" +
                                  text.Substring(lineLengthLimit);
                }
            }

            TextBlock textBlock = new TextBlock
            {
                Text = displayText,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center
            };
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size textSize = textBlock.DesiredSize;

            Canvas.SetLeft(textBlock, x - (textSize.Width / 2));
            Canvas.SetTop(textBlock, y - (textSize.Height / 2));

            VisualizationCanvas.Children.Add(ellipse);
            VisualizationCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// Допоміжний метод, що малює лінію між двома точками.
        /// </summary>
        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = Brushes.Gray,
                StrokeThickness = 2
            };
            VisualizationCanvas.Children.Add(line);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Перевіряємо, чи клік був саме лівою кнопкою
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to exit the program?",
                "Exit Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }
}