using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nodify;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NodifyTestWpf
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            var editor = new NodifyEditor();
            var editorVM = new EditorViewModel();
            editor.DataContext = editorVM;

            editor.SetBinding(NodifyEditor.ItemsSourceProperty, new Binding(nameof(EditorViewModel.Nodes)));
            editor.SetBinding(NodifyEditor.ConnectionsProperty, new Binding(nameof(EditorViewModel.Connections)));
            editor.SetBinding(NodifyEditor.PendingConnectionProperty, new Binding(nameof(EditorViewModel.PendingConnection)));
            editor.SetBinding(NodifyEditor.DisconnectConnectorCommandProperty, new Binding(nameof(EditorViewModel.DisconnectConnectorCommand)));

            var style = new Style(typeof(ItemContainer));
            style.Setters.Add(new Setter(ItemContainer.LocationProperty, new Binding(nameof(NodeViewModel.Location))));
            editor.ItemContainerStyle = style;

            editor.ItemTemplate = CreateNodeTemplate();
            editor.ConnectionTemplate = CreateConnectionTemplate();
            editor.PendingConnectionTemplate = CreatePendingConnectionTemplate();

            var (smallGrid, largeGrid, background) = CreateGridBrushes(editor);

            var root = new Grid { Background = background };
            editor.Background = smallGrid;

            var gridLayer = new Grid { Background = largeGrid };
            Panel.SetZIndex(gridLayer, -2);

            root.Children.Add(gridLayer);
            root.Children.Add(editor);

            Content = root;
        }
        private (Brush small, Brush large, Brush background) CreateGridBrushes(NodifyEditor editor)
        {
            var gridLineBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            var backgroundBrush = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            var smallGeometry = new GeometryDrawing
            {
                Geometry = Geometry.Parse("M0,0 L0,1 0.03,1 0.03,0.03 1,0.03 1,0 Z"),
                Brush = gridLineBrush
            };

            var largeGeometry = new GeometryDrawing
            {
                Geometry = Geometry.Parse("M0,0 L0,1 0.015,1 0.015,0.015 1,0.015 1,0 Z"),
                Brush = gridLineBrush
            };

            DrawingBrush CreateBrush(GeometryDrawing geometry, double size, double opacity)
            {
                var brush = new DrawingBrush
                {
                    Drawing = geometry,
                    TileMode = TileMode.Tile,
                    ViewportUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 0, size, size),
                    Opacity = opacity
                };
                BindingOperations.SetBinding(brush, DrawingBrush.TransformProperty, new Binding("ViewportTransform") { Source = editor });

                return brush;
            }

            return (CreateBrush(smallGeometry, 20, 1.0), CreateBrush(largeGeometry, 100, 0.5), backgroundBrush);
        }

        private DataTemplate CreateConnectionTemplate()
        {
            var template = new DataTemplate(typeof(ConnectionViewModel));
            var connectionFactory = new FrameworkElementFactory(typeof(Connection));
            connectionFactory.SetBinding(Connection.SourceProperty, new Binding("Source.Anchor"));
            connectionFactory.SetBinding(Connection.TargetProperty, new Binding("Target.Anchor"));
            template.VisualTree = connectionFactory;
            return template;
        }

        private DataTemplate CreatePendingConnectionTemplate()
        {
            var template = new DataTemplate(typeof(PendingConnectionViewModel));
            var pendingConnectionFactory = new FrameworkElementFactory(typeof(PendingConnection));
            pendingConnectionFactory.SetBinding(PendingConnection.StartedCommandProperty, new Binding(nameof(PendingConnectionViewModel.StartCommand)));
            pendingConnectionFactory.SetBinding(PendingConnection.CompletedCommandProperty, new Binding(nameof(PendingConnectionViewModel.FinishCommand)));
            pendingConnectionFactory.SetValue(PendingConnection.AllowOnlyConnectorsProperty, true);
            template.VisualTree = pendingConnectionFactory;
            return template;
        }

        private DataTemplate CreateNodeTemplate()
        {
            var template = new DataTemplate(typeof(NodeViewModel));
            var nodeFactory = new FrameworkElementFactory(typeof(Node));

            nodeFactory.SetBinding(Node.HeaderProperty, new Binding(nameof(NodeViewModel.Title)));
            nodeFactory.SetBinding(Node.InputProperty, new Binding(nameof(NodeViewModel.Input)));
            nodeFactory.SetBinding(Node.OutputProperty, new Binding(nameof(NodeViewModel.Output)));

            var inputTemplate = new DataTemplate(typeof(ConnectorViewModel));
            var inputConnectorFactory = new FrameworkElementFactory(typeof(NodeInput));
            inputConnectorFactory.SetBinding(NodeInput.HeaderProperty, new Binding(nameof(ConnectorViewModel.Title)));
            inputConnectorFactory.SetBinding(NodeInput.AnchorProperty, new Binding(nameof(ConnectorViewModel.Anchor)) { Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            inputConnectorFactory.SetBinding(NodeInput.IsConnectedProperty, new Binding(nameof(ConnectorViewModel.IsConnected)));
            inputTemplate.VisualTree = inputConnectorFactory;
            nodeFactory.SetValue(Node.InputConnectorTemplateProperty, inputTemplate);

            var outputTemplate = new DataTemplate(typeof(ConnectorViewModel));
            var outputConnectorFactory = new FrameworkElementFactory(typeof(NodeOutput));
            outputConnectorFactory.SetBinding(NodeOutput.HeaderProperty, new Binding(nameof(ConnectorViewModel.Title)));
            outputConnectorFactory.SetBinding(NodeOutput.AnchorProperty, new Binding(nameof(ConnectorViewModel.Anchor)) { Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            outputConnectorFactory.SetBinding(NodeOutput.IsConnectedProperty, new Binding(nameof(ConnectorViewModel.IsConnected)));
            outputTemplate.VisualTree = outputConnectorFactory;
            nodeFactory.SetValue(Node.OutputConnectorTemplateProperty, outputTemplate);

            template.VisualTree = nodeFactory;
            return template;
        }
    }
}
