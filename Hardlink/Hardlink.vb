Imports System.IO
Imports System.Reflection

Module Hardlink

   Private strDestination As String
   Private strLinkDest As String
   Private strSource As String
   Private arrSources() As String
   Private strLogfile As String

   Private blnSrcIntro As Boolean
   Private blnDestIntro As Boolean
   Private blnLinkDstIntro As Boolean
   Private blnLogIntro As Boolean

   Dim sFileWriter As System.IO.TextWriter

   <DllImport("Kernel32.dll", CharSet:=CharSet.Unicode)> _
   Private Function CreateHardLink(lpFileName As String, lpExistingFileName As String, lpSecurityAttributes As Pointer) As Boolean
   End Function

   Public Sub Main()

      Dim strArg As String

      ' read My.Application.CommandLineArgs 
      For Each strArg In My.Application.CommandLineArgs
         Select Case strArg
            Case "-d", "--destination"
               blnDestIntro = True
               blnSrcIntro = False
            Case "-t", "--link-dest"
               blnLinkDstIntro = True
               blnSrcIntro = False
            Case "-s", "--source-folder"
               blnSrcIntro = True
            Case "-l", "--logfile"
               blnLogIntro = True
               blnSrcIntro = False
            Case "-h", "-?", "--help"
               Console.Write(My.Resources.Help)
               End
            Case Else
               If blnSrcIntro = True Then
                  strSource &= "|" & strArg
               ElseIf blnDestIntro = True Then
                  strDestination = strArg
                  blnDestIntro = False
               ElseIf blnLinkDstIntro = True Then
                  strLinkDest = strArg
                  blnLinkDstIntro = False
               ElseIf blnLogIntro = True Then
                  strLogfile = strArg
                  blnLogIntro = False
               Else
                  Throw New ArgumentException("Ungültige Parameter: ", strArg)
               End If
         End Select
      Next

      sFileWriter = System.IO.TextWriter.Synchronized(System.IO.File.AppendText(strLogfile))
      AppendLog(vbCrLf & "-- Kopierte Dateien:")

      arrSources = Split(Right(strSource, Len(strSource) - 1), "|")
      CopyFolder(arrSources, strDestination, strLinkDest, True)

      AppendLog(vbCrLf & vbCrLf)
      Console.Write("Finished!")

   End Sub
   Private Sub CopyFolder(ByRef Sources() As String, ByRef Destination As String, LinkDestination As String, FirstLevel As Boolean)

      Dim strDir As String
      Dim strDest As String
      Dim strLinkDest As String = ""
      Dim MyTasks(Sources.Length - 1) As Tasks.Task
      Dim objState As Object
      Dim intX As Integer = 0

      For Each strDir In Sources
         If FirstLevel = True Then
            strDest = Path.Combine(Destination, Replace(strDir, ":", ""))
            If Not LinkDestination = "" Then strLinkDest = Path.Combine(LinkDestination, Replace(strDir, ":", ""))
         Else
            strDest = Path.Combine(Destination, strDir.Substring(strDir.LastIndexOf("\") + 1))
            If Not LinkDestination = "" Then strLinkDest = Path.Combine(LinkDestination, strDir.Substring(strDir.LastIndexOf("\") + 1))
         End If

         ' Verzeichnis im Ziel anlegen
         Directory.CreateDirectory(strDest)
         ' Unterverzeichnisse anlegen
         CopyFolder(Directory.GetDirectories(strDir), strDest, strLinkDest, False)

         ' prepare parameter values being passed as an array via the state-object
         objState = New Object() {Directory.GetFiles(strDir), strDest, strLinkDest}

         MyTasks(intX) = Tasks.Task.Factory.StartNew(Sub(State) Hardlink.CopyFiles( _
                                                                     CType(CType(State, Object())(0), String()), _
                                                                     CType(CType(State, Object())(1), String), _
                                                                     CType(CType(State, Object())(2), String)), _
                                                     objState)
         'MyTasks(intX) = New Tasks.Task(Of String)(Function(State) Hardlink.CopyFiles( _
         '                                                            CType(CType(State, Object())(0), String()), _
         '                                                            CType(CType(State, Object())(1), String), _
         '                                                            CType(CType(State, Object())(2), String)), _
         '                                          objState)
         'MyTasks(intX).RunSynchronously()

         intX += 1
      Next

      'Console.WriteLine("Warte auf " & Sources.Length & " Verzeichnisse...")

      Tasks.Task.WaitAll(MyTasks)

   End Sub
   Private Sub CopyFiles(ByRef Files() As String, ByRef Destination As String, ByRef LinkDest As String)

      Dim strFile As String = ""
      Dim objFileInfo As FileInfo
      Dim objFileLinkInfo As FileInfo
      Dim strRet As String = ""
      Dim strLinkFile As String = ""
      Dim strDestfile As String = ""

      For Each strFile In Files
         strDestfile = Path.Combine(Destination, strFile.Substring(strFile.LastIndexOf("\") + 1))
         Try
            If Not LinkDest = "" Then
               strLinkFile = Path.Combine(LinkDest, strFile.Substring(strFile.LastIndexOf("\") + 1))

               If File.Exists(strLinkFile) Then
                  objFileInfo = New FileInfo(strFile)
                  objFileLinkInfo = New FileInfo(strLinkFile)
                  ' compare modify-date and file-size
                  If objFileInfo.LastWriteTime = objFileLinkInfo.LastWriteTime And _
                     objFileInfo.Length = objFileLinkInfo.Length Then
                     ' if equal, create hardlink
                     CreateHardLink(strDestfile, strLinkFile, Nothing)
                  Else
                     File.Copy(strFile, strDestfile)
                     strRet &= strFile & vbCrLf
                  End If
               Else
                  File.Copy(strFile, strDestfile)
                  strRet &= strFile & vbCrLf
               End If
            Else
               File.Copy(strFile, strDestfile)
               strRet &= strFile & vbCrLf
            End If

         Catch ex As Exception
            Console.Error.WriteLine("Fehler bei """ & strFile & ": " & ex.Message)
            strRet &= "Fehler bei """ & strFile & ": " & ex.Message & vbCrLf
         End Try
      Next

      If strRet.Length > 0 Then
         AppendLog(strRet)
      End If

   End Sub
   Private Sub AppendLog(ByRef Text As String)

      sFileWriter.WriteLine(Text)

   End Sub
End Module
