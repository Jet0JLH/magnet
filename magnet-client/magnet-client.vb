Module magnet_client
    Sub wl(ByVal text As String, Optional ByVal mode As wlModes = 0, Optional ByVal forceLog As Boolean = False) 'WriteLine
        Select Case mode
            Case wlModes.Info
                Console.ForegroundColor = ConsoleColor.White
            Case wlModes.Warning
                Console.ForegroundColor = ConsoleColor.Yellow
            Case wlModes.Critical
                Console.ForegroundColor = ConsoleColor.Red
        End Select
        Console.WriteLine(text)
        Console.ForegroundColor = ConsoleColor.White
    End Sub
    Enum wlModes 'WriteLine Modes
        Info = 0
        Warning = 1
        Critical = 2
    End Enum
    Sub Main()

    End Sub

End Module
