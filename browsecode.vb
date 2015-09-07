Module Module1
    Private Const STEP1URL As String = "http://polldaddy.com/n/bbeae4e730c2ebcf23f9fa74a3fbc4e2" '"http://polldaddy.com/n/9291144d3af2f615f6f3144880ddf0af"
    Private Const POLLID As String = "8355767" '"8339159"
    Private Const VOTEA As String = "38014255" '"37936500"
    Private Const FINALURL As String = "http%3A//www.pennlive.com/sports/index.ssf/2014/10/which_band_should_win_the_marc.html" '"http%3A//www.pennlive.com/sports/index.ssf/2014/09/central_dauphin_pine_gove_marching_band_brawl.html"

    Sub Main(args() As String)
        Dim dtStop As Date = New Date(2014, 10, 29, 23, 0, 0)
        While Now < dtStop
            If VoteOnce() Then
                Console.WriteLine("Vote Submitted.")
                Threading.Thread.Sleep(5000)
            Else
                Console.WriteLine("Vote Failed.")
                Threading.Thread.Sleep(65 * 60 * 1000)
            End If
        End While
    End Sub
    Public NotInheritable Class Evaluator
        Private Shared mScriptControl As MSScriptControl.ScriptControlClass
        Shared Sub New()
            mScriptControl = New MSScriptControl.ScriptControlClass
            mScriptControl.Language = "Javascript"
            mScriptControl.AllowUI = False
        End Sub
        Public Shared Function Evaluate(ByVal s As String) As Object
            mScriptControl.Reset()
            Return mScriptControl.Eval(s)
        End Function
    End Class
    Public Function VoteOnce() As Boolean
        Dim step1location As String = Evaluator.Evaluate("'" & STEP1URL & "/" & POLLID & "?' + (new Date).getTime();")
        Dim res As String = Browse(step1location).Split(";")(0) & ";PDV_n" & POLLID & ";"
        Dim n As String = Evaluator.Evaluate(res)
        Dim step2location As String = "http://polls.polldaddy.com/vote-js.php?p=" & POLLID & "&b=0&a=" & VOTEA & ",&o=&va=16&cookie=0&n=" & n & "&url=" & FINALURL
        Dim response As String = Browse(step2location)
        Return Not response.ToLower.StartsWith("alert(")
    End Function
    Public Function Browse(ByVal URL As String) As String
        Dim HttpWebReq As System.Net.HttpWebRequest = System.Net.WebRequest.Create(URL)
        Dim HttpWebResponse As System.Net.HttpWebResponse = HttpWebReq.GetResponse
        Dim ResponseStream As System.IO.Stream = HttpWebResponse.GetResponseStream
        Dim StreamReader As New System.IO.StreamReader(ResponseStream)
        Dim res As String
        res = StreamReader.ReadToEnd
        Return res
    End Function

End Module
