Imports HtmlAgilityPack
Imports OpenQA.Selenium.Chrome
Imports OpenQA.Selenium
Imports System.Xml

Public Class Form1
    Public GameList As New List(Of GameData)
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim scrapedData As String = ScrapeWebpage("https://lockervision.nba.com/")

        For Each s As String In FilterDivElements(ExtractDivElements(scrapedData))
            makeGameData(ParseVisibleText(s))
        Next
        WriteGameDataToXmlFile(GameList, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\JersyData.xml")
        Me.Close()
    End Sub
    Public Function makeGameData(Rawlist As List(Of String))
        Dim GD As New GameData
        GD.Team1 = Rawlist(0)
        GD.Team1Uniform = Rawlist(2)
        GD.Team2 = Rawlist(1)
        GD.Team2Uniform = Rawlist(4)
        GD.Arena = Rawlist(9)
        GD.GameDate = Convert.ToDateTime(Rawlist(11))
        GD.Time = Rawlist(12) + " " + Rawlist(13)
        GD.Location = Rawlist(10)
        GameList.Add(GD)
    End Function
    Public Function ScrapeWebpage(url As String) As String

        'Configure ChromeDriver options
        Dim options As New ChromeOptions()
        options.AddArgument("--headless")
        options.AddArgument("--disable-gpu")
        options.AddArgument("--disable-extensions")
        options.AddArgument("--no-sandbox")
        options.AddArgument("--disable-dev-shm-usage")

        'Create new ChromeDriver instance
        Dim driver As IWebDriver = New ChromeDriver(options)

        'Navigate to webpage URL
        driver.Navigate().GoToUrl(url)

        'Get HTML content of webpage
        Dim htmlContent As String = driver.PageSource

        'Quit ChromeDriver instance
        driver.Quit()

        'Return scraped data
        Return htmlContent


    End Function
    Public Function ExtractDivElements(htmlContent As String) As String()

        'Extract div elements 
        Dim doc As New HtmlAgilityPack.HtmlDocument()
        doc.LoadHtml(htmlContent)
        Dim divNodes As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@class='slick-slide']")
        Dim divNodes2 As HtmlNodeCollection = doc.DocumentNode.SelectNodes("//div[@class='slick-list']")
        Dim divNodes3 As New HtmlNodeCollection(Nothing)

        For Each node As HtmlNode In divNodes2(0).SelectNodes("./div")(0).SelectNodes("./div")
            divNodes3.Add(node)
        Next


        'Convert div elements to List of strings
        Dim divStrings As New List(Of String)()
        For Each divNode3 As HtmlNode In divNodes3
            divStrings.Add(divNode3.OuterHtml)
        Next

        'Return scraped data
        Return divStrings.ToArray()

    End Function
    Public Function FilterDivElements(divElements As String()) As String()
        'This only filters off the last Div
        'Filter div elements to only those containing the word "Shop"
        Dim filteredDivs As New List(Of String)()
        For Each divElement As String In divElements
            If divElement.Contains("Shop") Then
                filteredDivs.Add(divElement)
            End If
        Next

        'Return filtered div elements
        'Return filteredDivs.ToArray()

    End Function
    Public Function ParseVisibleText(htmlString As String) As List(Of String)


        'Load HTML string into HtmlDocument
        Dim doc As New HtmlDocument()
        doc.LoadHtml(htmlString)
        Dim DataList As New List(Of String)
        'Get all visible text nodes and concatenate their inner text
        Dim visibleText As String = ""
        For Each node As HtmlNode In doc.DocumentNode.DescendantsAndSelf.Where(Function(n) n.NodeType = HtmlNodeType.Text AndAlso n.ParentNode.Name <> "script" AndAlso n.ParentNode.Name <> "style" AndAlso n.ParentNode.Name <> "noscript" AndAlso n.ParentNode.Name <> "head" AndAlso n.ParentNode.Name <> "meta")
            visibleText &= node.InnerText.Trim() & " "
            DataList.Add(node.InnerText)
        Next


        'Return the array of visible text strings
        Return DataList
    End Function
    Public Class GameData
        Public Property Team1 As String
        Public Property Team1Uniform As String
        Public Property Team1UniformImage As String 'can parse for this if you want the image link to the jersey
        Public Property Team2 As String
        Public Property Team2Uniform As String
        Public Property Team2UniformImage As String 'can parse for this if you want the image link to the jersey
        Public Property Arena As String
        Public Property Location As String
        Public Property GameDate As Date
        Public Property Time As String
    End Class
    Public Sub WriteGameDataToXmlFile(gameDataList As List(Of GameData), filename As String)

        'Create a new XML document
        Dim doc As New XmlDocument()

        'Create the root element and add it to the document
        Dim root As XmlElement = doc.CreateElement("games")
        doc.AppendChild(root)

        'Loop through the list of GameData objects and create a new XML element for each one
        For Each gameData As GameData In gameDataList
            Dim gameElement As XmlElement = doc.CreateElement("game")

            'Add child elements for each property of the GameData object
            Dim team1Element As XmlElement = doc.CreateElement("team1")
            team1Element.InnerText = gameData.Team1
            gameElement.AppendChild(team1Element)

            Dim team1UniformElement As XmlElement = doc.CreateElement("team1Uniform")
            team1UniformElement.InnerText = gameData.Team1Uniform
            gameElement.AppendChild(team1UniformElement)

            Dim team2Element As XmlElement = doc.CreateElement("team2")
            team2Element.InnerText = gameData.Team2
            gameElement.AppendChild(team2Element)

            Dim team2UniformElement As XmlElement = doc.CreateElement("team2Uniform")
            team2UniformElement.InnerText = gameData.Team2Uniform
            gameElement.AppendChild(team2UniformElement)

            Dim arenaElement As XmlElement = doc.CreateElement("arena")
            arenaElement.InnerText = gameData.Arena
            gameElement.AppendChild(arenaElement)

            Dim locationElement As XmlElement = doc.CreateElement("location")
            locationElement.InnerText = gameData.Location
            gameElement.AppendChild(locationElement)

            Dim dateElement As XmlElement = doc.CreateElement("date")
            dateElement.InnerText = gameData.GameDate.ToString("D")
            gameElement.AppendChild(dateElement)

            Dim TimeElement As XmlElement = doc.CreateElement("time")
            TimeElement.InnerText = gameData.Time
            gameElement.AppendChild(TimeElement)

            'Add the game element to the root element
            root.AppendChild(gameElement)
        Next

        'Save the XML document to a file
        doc.Save(filename)

    End Sub
End Class
