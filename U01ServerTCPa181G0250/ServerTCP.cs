using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using U01ClienteTCPa181G0250;

namespace U01ServerTCPa181G0250
{
    public class ServerTCP : INotifyPropertyChanged
    {
        TcpListener listener;

        List<TcpClient> clients = new List<TcpClient>();

        public ICommand IniciarCommand { get; set; }
        public ICommand DetenerCommand { get; set; }

        Dispatcher dispatcher;


        public ServerTCP()
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            IniciarCommand = new RelayCommand(Iniciar);
            DetenerCommand = new RelayCommand(Detener);

            string filename = "votosGuardados.json";

            if (File.Exists(filename)) 
            { string json = File.ReadAllText(filename); 
              List<byte> datos = JsonSerializer.Deserialize<List<byte>>(json); 
              VotoRojo = datos[2];
              VotoAmarillo = datos[1];
              VotoVerde = datos[0];
            }
            else { Guardar(); } 

            //string jsonString = File.ReadAllText(fileName);
            //WeatherForecast weatherForecast = JsonSerializer.Deserialize<WeatherForecast>(jsonString);
        }

        private int votoRojo;

        public int VotoRojo
        {
            get { return votoRojo; }
            set
            {
                votoRojo = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VotoRojo"));
            }
        }

        private int votoAma;

        public int VotoAmarillo
        {
            get { return votoAma; }
            set
            {
                votoAma = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VotoAmarillo"));
            }
        }

        private int votoVer;

        public int VotoVerde
        {
            get { return votoVer; }
            set
            {
                votoVer = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("VotoVerde"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Iniciar()
        {
            if (listener == null)
            {
                Task.Run(() =>
                { 
                    try
                    {
                        IPEndPoint ep = new IPEndPoint(IPAddress.Any, 5000);
                        listener = new TcpListener(ep);
                        listener.Start();

                        while (listener != null)
                        {
                            TcpClient tcp = listener.AcceptTcpClient();
                            clients.Add(tcp);

                            Recibir(tcp);
                        }
                    }
                catch (Exception) { }
            });
            }
        }

        private void Recibir(TcpClient cliente)
        {

            Task.Run(() =>
            {

                NetworkStream ns = cliente.GetStream();

                while(cliente.Connected)
                {
                    if (cliente.Available > 0)
                    {

                        //byte[] buffer = new byte[cliente.Available];
                        //ns.ReadByte();

                        EstadosEva voto = (EstadosEva)ns.ReadByte();
                        if(voto == EstadosEva.votoPositivo) { VotoVerde += 1; } 
                        else if(voto == EstadosEva.votoRegular ) { VotoAmarillo += 1; }
                        else if (voto == EstadosEva.votoMalo) { VotoRojo += 1; }
                        else { VotoAmarillo += 1; } 
                    }
                    Task.Delay(100); 
                }

            });
        }

        public void Detener()
        {
            if (listener != null)
            {
                listener.Stop();
                listener = null;

                foreach (var cliente in clients)
                {
                    cliente.Close();
                }

                Guardar();

                clients.Clear();


            }
        }

        public void Guardar()
        {
            var list = new List<int> { VotoVerde, VotoAmarillo, VotoRojo };

            string filename = "votosGuardados.json";
            string JsonString = JsonSerializer.Serialize(list);
            File.WriteAllText(filename, JsonString);

        }



    }
}
