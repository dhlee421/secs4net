﻿using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Secs4Net
{
    internal static class SocketAsyncExtension
    {
        private static readonly Task<bool> ConnectAsyncResultCache = Task.FromResult(true);

        internal static Task<bool> ConnectAsync(this Socket socket, IPAddress target, int port)
        {
            var tcs = new TaskCompletionSource<bool>();
            var ce = new SocketAsyncEventArgs { RemoteEndPoint = new IPEndPoint(target, port), UserToken = tcs };
            ce.Completed += ConnectCompleted;
            if (socket.ConnectAsync(ce))
                return tcs.Task;
            return ConnectAsyncResultCache;
        }

        private static void ConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            var tcs2 = Unsafe.As<TaskCompletionSource<bool>>(e.UserToken);
            if (e.SocketError == SocketError.Success)
            {
                tcs2.SetResult(e.ConnectSocket != null);
            }
            else
            {
                tcs2.SetException(new SocketException((int)e.SocketError));
            }
        }

        internal static Task<Socket> AcceptAsync(this Socket socket)
        {
            var tcs = new TaskCompletionSource<Socket>();
            var ce = new SocketAsyncEventArgs { UserToken = tcs };
            ce.Completed += AcceptCompleted;
            if (socket.AcceptAsync(ce))
                return tcs.Task;
            return Task.FromResult(ce.AcceptSocket);
        }

        private static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            var tcs2 = Unsafe.As<TaskCompletionSource<Socket>>(e.UserToken);
            if (e.AcceptSocket != null && e.SocketError == SocketError.Success)
            {
                tcs2.SetResult(e.AcceptSocket);
            }
            else
            {
                tcs2.SetException(new SocketException((int)e.SocketError));
            }
        }
    }
}