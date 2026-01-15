using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
//using Netproto;
using Google.Protobuf;
using System.Buffers.Binary;

public class NetManager
{
    //private TcpClient _client;
    //private NetworkStream _stream;

    //public async Task<bool> ConnectToGameServer()
    //{
    //    try
    //    {
    //        _client = new TcpClient();
    //        await _client.ConnectAsync(Config.GameServerHost, Config.GameServerPort);
    //        _stream = _client.GetStream();
    //        Debug.Log("게임 서버에 성공적으로 연결되었습니다.");
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"게임 서버 연결 실패: {ex.Message}");
    //        return false;
    //    }
    //}

    //public async Task SendMessageAsync(IMessage msg)
    //{
    //    if (_stream == null)
    //    {
    //        Debug.LogError("네트워크 스트림이 초기화되지 않았습니다.");
    //        return;
    //    }

    //    var envelope = new Envelope
    //    {
    //        ReqId = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
    //        Type = msg.GetType().Name,
    //        Payload = msg.ToByteString()
    //    };

    //    using (var ms = new MemoryStream())
    //    {
    //        envelope.WriteTo(ms);
    //        byte[] data = ms.ToArray();

    //        byte[] lengthPrefix = new byte[4];
    //        BinaryPrimitives.WriteInt32LittleEndian(lengthPrefix, data.Length);

    //        await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
    //        await _stream.WriteAsync(data, 0, data.Length);
    //        Debug.Log($"메시지 전송: {msg.GetType().Name}");
    //    }
    //}

    //public async Task<Envelope> ReceiveMessageAsync()
    //{
    //    if (_stream == null)
    //    {
    //        Debug.LogError("네트워크 스트림이 초기화되지 않았습니다.");
    //        return null;
    //    }
    //    byte[] lengthPrefix = new byte[4];
    //    int bytesRead = await _stream.ReadAsync(lengthPrefix, 0, lengthPrefix.Length);
    //    if (bytesRead < lengthPrefix.Length)
    //    {
    //        Debug.LogError("메시지 길이 읽기 실패");
    //        return null;
    //    }

    //    int messageLength = BinaryPrimitives.ReadInt32LittleEndian(lengthPrefix);
    //    byte[] messageData = new byte[messageLength];
    //    bytesRead = 0;
    //    while (bytesRead < messageLength)
    //    {
    //        int read = await _stream.ReadAsync(messageData, bytesRead, messageLength - bytesRead);
    //        if (read <= 0)
    //        {
    //            Debug.LogError("메시지 데이터 읽기 실패");
    //            return null;
    //        }
    //        bytesRead += read;
    //    }
    //    var envelope = Envelope.Parser.ParseFrom(messageData);
    //    Debug.Log($"메시지 수신: {envelope.Type}");
    //    return envelope;
    //}

    //public async Task<LoginRes> LoginAsync(string accessJwt)
    //{
    //    var req = new LoginReq { AccessJwt = accessJwt };
    //    await SendMessageAsync(req);

    //    try
    //    {
    //        var envelope = await ReceiveMessageAsync();
    //        if (envelope.Type == nameof(LoginRes))
    //        {
    //            var res = LoginRes.Parser.ParseFrom(envelope.Payload);
    //            Debug.Log($"[NetManager] 로그인 결과: {res.Success}, {res.DisplayName}");
    //            return res;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError(ex.Message);
    //        Disconnect();
    //    }
    //    return null;
    //}

    //public void Disconnect()
    //{
    //    try
    //    {
    //        _stream?.Close();
    //        _client?.Close();
    //        Debug.Log("[NetManager] 서버 연결 종료");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError($"[NetManager] 연결 종료 실패: {ex.Message}");
    //    }
    //}
}