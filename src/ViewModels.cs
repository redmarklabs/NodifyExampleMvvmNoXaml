using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace NodifyTestWpf
{
    public partial class ConnectorViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private Point _anchor;

        [ObservableProperty]
        private bool _isConnected;
    }

    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ConnectorViewModel> _input = new();

        [ObservableProperty]
        private ObservableCollection<ConnectorViewModel> _output = new();

        [ObservableProperty]
        private Point _location = new();
    }

    public class ConnectionViewModel : ObservableObject
    {
        public ConnectorViewModel Source { get; }
        public ConnectorViewModel Target { get; }

        public ConnectionViewModel(ConnectorViewModel source, ConnectorViewModel target)
        {
            Source = source;
            Target = target;

            Source.IsConnected = true;
            Target.IsConnected = true;
        }
    }

    public partial class PendingConnectionViewModel : ObservableObject
    {
        private readonly EditorViewModel _editor;
        private ConnectorViewModel? _source;

        public PendingConnectionViewModel(EditorViewModel editor)
        {
            _editor = editor;
        }

        [RelayCommand]
        private void Start(ConnectorViewModel source)
        {
            _source = source;
        }

        [RelayCommand]
        private void Finish(ConnectorViewModel target)
        {
            if (target != null && _source != null) _editor.Connect(_source, target);
        }
    }

    public partial class EditorViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<NodeViewModel> _nodes = new();
        [ObservableProperty]
        private ObservableCollection<ConnectionViewModel> _connections = new();
        [ObservableProperty]
        private PendingConnectionViewModel _pendingConnection;

        [RelayCommand]
        private void DisconnectConnector(ConnectorViewModel connector)
        {
            var toRemove = Connections
                .Where(x => x.Source == connector || x.Target == connector)
                .ToList();

            foreach (var connection in toRemove)
            {
                connection.Source.IsConnected = false;
                connection.Target.IsConnected = false;
                Connections.Remove(connection);
            }
        }

        public void Connect(ConnectorViewModel source, ConnectorViewModel target)
        {
            Connections.Add(new ConnectionViewModel(source, target));
        }

        public EditorViewModel()
        {
            PendingConnection = new PendingConnectionViewModel(this);

            var welcome = new NodeViewModel
            {
                Title = "Welcome",
                Input = new ObservableCollection<ConnectorViewModel>
                {
                    new ConnectorViewModel
                    {
                        Title = "In"
                    }
                },
                Output = new ObservableCollection<ConnectorViewModel>
                {
                    new ConnectorViewModel
                    {
                        Title = "Out"
                    }
                },
                Location = new Point(0, 0),
            };

            var nodify = new NodeViewModel
            {
                Title = "To Nodify",
                Input = new ObservableCollection<ConnectorViewModel>
                {
                    new ConnectorViewModel
                    {
                        Title = "In"
                    }
                },
                Location = new Point(50, 50),
            };

            Nodes.Add(welcome);
            Nodes.Add(nodify);

            Connect(welcome.Output[0], nodify.Input[0]);
        }
    }
}
