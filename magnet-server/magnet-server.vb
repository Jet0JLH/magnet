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
    Const configPath As String = "magnet-server.conf"
    Dim logPath As String = ""
    Dim port As String = ""
    Sub Main()
        wl("Set Workingdirectory...")
        My.Computer.FileSystem.CurrentDirectory = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
        wl("Check config...")
        If checkConfig() <> 0 Then wl("Generate config...") : If generateConfig() <> 0 Then wl("Unable to generate config", wlModes.Critical, True)
        wl("Startup magnet-server v" & My.Application.Info.Version.ToString, wlModes.Info, False, True)
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
                            port = .Element("port")
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

End Module
