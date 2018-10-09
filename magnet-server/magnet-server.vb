Imports System.Net.Sockets
Imports System.IO
Imports System.Net
Module magnet_server
    Sub wl(ByVal text As String, Optional ByVal mode As wlModes = 0, Optional ByVal isFatal As Boolean = False, Optional ByVal forceLog As Boolean = False) 'WriteLine
        Select Case mode
            Case wlModes.Info
                Console.ForegroundColor = ConsoleColor.White
            Case wlModes.Warning
                Console.ForegroundColor = ConsoleColor.Yellow
            Case wlModes.Critical
                Console.ForegroundColor = ConsoleColor.Red
        End Select
        Console.WriteLine(text)
        If isFatal Then Stop
        Console.ForegroundColor = ConsoleColor.White
    End Sub
    Enum wlModes 'WriteLine Modes
        Info = 0
        Warning = 1
        Critical = 2
    End Enum
#Region "Config"
    Const configPath As String = "magnet-server.conf"
    Dim logPath As String = ""
#End Region
#Region "TCP-Server"
    Dim doTcpService As Boolean
    Dim tcp_thread As Threading.Thread
    Dim tcp_server As TcpListener
    Dim tcp_client As TcpClient
    Dim tcp_ipendpoint As IPEndPoint
    Dim connections As List(Of Connection)
    Dim tcp_port As String = ""
    Structure Connection
        Dim stream As NetworkStream
        Dim streamw As StreamWriter
        Dim streamr As StreamReader
        Dim rep As IPEndPoint 'RemoteEndPoint
    End Structure
#End Region

    Sub Main()
        wl("Set Workingdirectory...")
        My.Computer.FileSystem.CurrentDirectory = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        wl("Check config...")
        If checkConfig() <> 0 Then wl("Generate config...") : If generateConfig() <> 0 Then wl("Unable to generate config", wlModes.Critical, True)
        wl("Startup magnet-server v" & My.Application.Info.Version.ToString, wlModes.Info, False, True)
        wl("Initialize variables")
        initializeVars()
        wl("Startup TCP Interface...")
        tcp_thread.Start()
    End Sub
    Function checkConfig() As Byte
        If My.Computer.FileSystem.FileExists(configPath) Then
            Try
                Dim tempXML As XDocument = XDocument.Load(configPath)
                Try
                    Dim temp As String = ""
                    With tempXML.Element("conf")
                        logPath = .Element("logPath").Value
                        With .Element("service")
                            tcp_port = .Element("port")
                        End With
                    End With

                    Return 0
                Catch ex As Exception
                    Return 3
                End Try
            Catch ex As Exception
                Return 2
            End Try
        Else
            Return 1
        End If
    End Function
    Function generateConfig() As Byte
        Dim tempXML As New XDocument(<conf><logPath>magnet-server.log</logPath><service><port>8894</port></service></conf>)
        Try
            tempXML.Save(configPath)
            Return 0
        Catch ex As Exception
            Return 1
        End Try
    End Function
    Sub initializeVars()
        tcp_ipendpoint = New IPEndPoint(IPAddress.Any, tcp_port)
        tcp_client = New TcpClient
        connections = New List(Of Connection)
        tcp_thread = New Threading.Thread(AddressOf tcp_server_thread)
        doTcpService = True
    End Sub

#Region "TCP functions"
    Private Sub tcp_server_thread()
        tcp_server = New TcpListener(tcp_ipendpoint)
        tcp_server.Start()
        While doTcpService 'Wait for new Clients while Service shut be stoped
            tcp_client = tcp_server.AcceptTcpClient
            Dim c As New Connection 'create new connection for then new client
            c.stream = tcp_client.GetStream
            c.streamr = New StreamReader(c.stream)
            c.streamw = New StreamWriter(c.stream)
            c.rep = tcp_client.Client.RemoteEndPoint
            wl("New client connection" & vbCrLf &
               "Adress: " & c.rep.Address.ToString & vbCrLf &
               "Port: " & c.rep.Port & vbCrLf &
               "Family: " & c.rep.AddressFamily.ToString)
            connections.Add(c)
            Dim t As New Threading.Thread(AddressOf ListenToConnection)
            t.Start(c)
        End While
    End Sub
    Private Sub ListenToConnection(ByVal con As Connection)
        Do
            Try
                If con.streamr.EndOfStream = True Then
                    connections.Remove(con)
                    Console.WriteLine(con.rep.Address.ToString & ":" & con.rep.Port & " has exit.") 'Connection closed
                    Exit Do
                Else
                    Dim tmp As String = con.streamr.ReadLine ' wait for message
                    Console.WriteLine(con.rep.Address.ToString & ":" & con.rep.Port & ": " & tmp)
                End If
            Catch ' the current connection has died.
                connections.Remove(con)
                Console.WriteLine(con.rep.Address.ToString & ":" & con.rep.Port & " has died.")
                Exit Do
            End Try
        Loop
    End Sub
    Private Sub SendToAllClients(ByVal s As String)
        For Each c As Connection In connections
            Try
                c.streamw.WriteLine(s)
                c.streamw.Flush()
            Catch
            End Try
        Next
    End Sub
#End Region
End Module
