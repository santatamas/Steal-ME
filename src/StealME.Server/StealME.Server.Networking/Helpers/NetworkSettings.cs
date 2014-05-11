using System;
using System.Configuration;
using System.Linq;
using System.Net;

namespace StealME.Server.Networking.Helpers
{
    public class NetworkSettings
    {
        #region Private State
        // the maximum number of connections the sample is designed to handle simultaneously 
        private Int32 _maxConnections;

        // this variable allows us to create some extra SAEA objects for the pool, if we wish.
        private Int32 _numberOfSaeaForRecSend;

        // max # of pending connections the listener can hold in queue
        private Int32 _backlog;

        // tells us how many objects to put in pool for accept operations
        private Int32 _maxSimultaneousAcceptOps;

        // buffer size to use for each socket receive operation
        private Int32 _receiveBufferSize;

        // length of message prefix for receive ops
        private Int32 _receivePrefixLength;

        // length of message prefix for send ops
        private Int32 _sendPrefixLength;

        // See comments in buffer manager.
        private Int32 _opsToPreAllocate;

        // Endpoint for the listener.
        private IPEndPoint _localEndPoint;
        private int _excessSaeaObjectsInPool;
        #endregion

        public int MaxConnections
        {
            get { return _maxConnections; }
            set { _maxConnections = value; }
        }
        public int NumberOfSaeaForRecSend
        {
            get { return MaxConnections + ExcessSaeaObjectsInPool; }
        }
        public int Backlog
        {
            get { return _backlog; }
            set { _backlog = value; }
        }
        public int MaxSimultaneousAcceptOps
        {
            get { return _maxSimultaneousAcceptOps; }
            set { _maxSimultaneousAcceptOps = value; }
        }
        public int ReceiveBufferSize
        {
            get { return _receiveBufferSize; }
            set { _receiveBufferSize = value; }
        }
        public int ReceivePrefixLength
        {
            get { return _receivePrefixLength; }
            set { _receivePrefixLength = value; }
        }
        public int SendPrefixLength
        {
            get { return _sendPrefixLength; }
            set { _sendPrefixLength = value; }
        }
        public int OpsToPreAllocate
        {
            get { return _opsToPreAllocate; }
            set { _opsToPreAllocate = value; }
        }
        public int ExcessSaeaObjectsInPool
        {
            get { return _excessSaeaObjectsInPool; }
            set { _excessSaeaObjectsInPool = value; }
        }        
        public IPEndPoint LocalEndPoint
        {
            get { return _localEndPoint; }
            set { _localEndPoint = value; }
        }
        public int BufferSize
        {
            get { return _receiveBufferSize; }
        }

        public static NetworkSettings GetFromAppConfig()
        {
            var result = new NetworkSettings();
            result.Backlog = int.Parse(ConfigurationManager.AppSettings["backLog"]);
            result.MaxConnections = int.Parse(ConfigurationManager.AppSettings["maxNumberOfConnections"]);
            result.ReceiveBufferSize = int.Parse(ConfigurationManager.AppSettings["bufferSize"]);
            result.ReceivePrefixLength = int.Parse(ConfigurationManager.AppSettings["receivePrefixLength"]);
            result.SendPrefixLength = int.Parse(ConfigurationManager.AppSettings["sendPrefixLength"]);
            result.MaxSimultaneousAcceptOps = int.Parse(ConfigurationManager.AppSettings["maxSimultaneousAcceptOps"]);
            result.OpsToPreAllocate = int.Parse(ConfigurationManager.AppSettings["opsToPreAlloc"]);
            result.ExcessSaeaObjectsInPool = int.Parse(ConfigurationManager.AppSettings["excessSaeaObjectsInPool"]);

            if (ConfigurationManager.AppSettings.AllKeys.Any(k => k.Equals("endpointAdr")))
            {
                result.LocalEndPoint = new IPEndPoint(
                    IPAddress.Parse(ConfigurationManager.AppSettings["endpointAdr"]),
                    int.Parse(ConfigurationManager.AppSettings["port"]));
            }
            else
            {
                result.LocalEndPoint = new IPEndPoint(
                   IPAddress.Any,
                   int.Parse(ConfigurationManager.AppSettings["port"]));
            }

            return result;
        }
    }
}