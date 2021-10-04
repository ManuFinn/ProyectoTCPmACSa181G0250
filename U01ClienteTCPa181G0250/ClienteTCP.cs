using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.Text.Json;

namespace U01ClienteTCPa181G0250
{

    [Serializable]

    public enum EstadosEva : byte { votoMalo, votoRegular, votoPositivo }

    public class ClienteTCP : INotifyPropertyChanged
    {
        TcpClient client = new TcpClient();

        public ObservableCollection<string> Evaluaciones { get; set; } = new();

        Dispatcher disp;

        private EstadosEva estadosE;

        public EstadosEva EstadosE
        {
            get { return estadosE; }
            set
            {
                estadosE = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EstadosE"));
            }
        }

        private string ip;

        public string IpServer
        {
            get { return ip; }
            set { ip = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ip"));
            }
        }

        private bool conectado = false;

        public bool Conectado
        {
            get { return conectado; }
            set { conectado = value; }
        }

        public ICommand ConectarCommand { get; set; }
        public ICommand EnviarCommand { get; set; }

        public ICommand VotarMaloCommand { get; set; }
        public ICommand VotarRegularCommand { get; set; }
        public ICommand VotarBuenoCommand { get; set; }
        public ClienteTCP()
        {
            disp = Dispatcher.CurrentDispatcher;
            ConectarCommand = new RelayCommand(Conectar);
            EnviarCommand = new RelayCommand(Enviar);

            VotarMaloCommand = new RelayCommand(VotarMalo);
            VotarRegularCommand = new RelayCommand(VotarRegular);
            VotarBuenoCommand = new RelayCommand(VotarBueno);
        }

        private void VotarMalo()
        {
            EstadosE = EstadosEva.votoMalo;
            Enviar();
        }
        private void VotarRegular()
        {
            EstadosE = EstadosEva.votoRegular;
            Enviar();
        }
        private void VotarBueno()
        {
            EstadosE = EstadosEva.votoPositivo;
            Enviar();
        }

        private void Conectar()
        {
            if (Conectado == false)
            {
                client.Connect(new IPEndPoint(IPAddress.Parse(ip), 5000));
                Task.Delay(10);
                Conectado = client.Connected;

                /*Task.Run(() =>
                {
                    Recibir(); 
                });
                */

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Conectado)));
            }
        }

        private void Enviar() 
        {
            NetworkStream ns = client.GetStream(); 
            // var buffer = Encoding.UTF8.GetBytes();

            ns.WriteByte((byte)EstadosE);
        }


        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
