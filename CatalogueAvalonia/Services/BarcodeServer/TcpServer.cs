using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CatalogueAvalonia.Core;
using CatalogueAvalonia.Models;

namespace CatalogueAvalonia.Services.BarcodeServer;

public class TcpServer
{
    public int Port = 0;
    private readonly TcpListener _server = new TcpListener(IPAddress.Any, 0);
    private readonly DataStore.DataStore _dataStore;

    public TcpServer(DataStore.DataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public async Task StartTcpServer()
    {
        try
        {
            _server.Start();
            Port = ((IPEndPoint)_server.LocalEndpoint).Port;
            while (true)
            {
                var tcpClient = await _server.AcceptTcpClientAsync();
                
                Task.Run(async ()=>await ProcessClientAsync(tcpClient));
            }
        }
        finally
        {
            _server.Stop();
        }
    }

    async Task ProcessClientAsync(TcpClient tcpClient)
    {
        var stream = tcpClient.GetStream();
        var response = new List<byte>();
        int bytesRead = 10;
        while (true)
        {
            byte[] sizeBuffer = new byte[4];
            await stream.ReadExactlyAsync(sizeBuffer, 0, sizeBuffer.Length);
            int size = BitConverter.ToInt32(sizeBuffer, 0);
            byte[] data = new byte[size];
            int bytes = await stream.ReadAsync(data);
            
            var input = Encoding.UTF8.GetString(data, 0, bytes);

            var res = new List<CatalogueModel>();
            
            if (input.Contains("Search"))
            {
                res = await TrySearchUniValue(input.Substring(input.IndexOf('|')+1));
            }
            
            if (input == "END") break;

            await stream.WriteAsync(res.Serialize());
            GC.Collect();
            response.Clear();
        }
        tcpClient.Close();
    }

    private async Task<List<CatalogueModel>> TrySearchUniValue(string value)
    {
        List<CatalogueModel> catalogueModels = new List<CatalogueModel>();
        await foreach (var res in DataFiltering.FilterByUniValue(_dataStore.CatalogueModels, value, new CancellationToken()))
            catalogueModels.Add(res);

        return catalogueModels;
    }
}