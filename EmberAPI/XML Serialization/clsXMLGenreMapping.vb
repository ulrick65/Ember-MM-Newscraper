' ################################################################################
' #                             EMBER MEDIA MANAGER                              #
' ################################################################################
' ################################################################################
' # This file is part of Ember Media Manager.                                    #
' #                                                                              #
' # Ember Media Manager is free software: you can redistribute it and/or modify  #
' # it under the terms of the GNU General Public License as published by         #
' # the Free Software Foundation, either version 3 of the License, or            #
' # (at your option) any later version.                                          #
' #                                                                              #
' # Ember Media Manager is distributed in the hope that it will be useful,       #
' # but WITHOUT ANY WARRANTY; without even the implied warranty of               #
' # MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the                #
' # GNU General Public License for more details.                                 #
' #                                                                              #
' # You should have received a copy of the GNU General Public License            #
' # along with Ember Media Manager.  If not, see <http://www.gnu.org/licenses/>. #
' ################################################################################

Imports Newtonsoft.Json
Imports NLog
Imports System.IO
Imports System.Xml.Serialization

<Serializable()>
<XmlRoot("core.genres")>
Public Class clsXMLGenreMapping
    Implements ICloneable

#Region "Fields"

    Shared _Logger As Logger = LogManager.GetCurrentClassLogger()

#End Region 'Fields

#Region "Properties"

    <XmlElement("defaultimage")>
    Public Property DefaultImage() As String = "default.jpg"

    <XmlIgnore>
    Public ReadOnly Property FileNameFullPath As String = String.Empty

    <XmlElement("genre")>
    Public Property Genres() As List(Of GenreProperty) = New List(Of GenreProperty)

    <XmlElement("mapping")>
    Public Property Mappings() As List(Of GenreMapping) = New List(Of GenreMapping)

#End Region 'Properties

#Region "Constructors"
    ''' <summary>
    ''' Needed for Load Sub
    ''' </summary>
    Private Sub New()
        Clear()
    End Sub

    Public Sub New(ByVal fileNameFullPath As String)
        Me.FileNameFullPath = fileNameFullPath
    End Sub

#End Region 'Constructors

#Region "Methods"

    Private Sub Clear()
        DefaultImage = "default.jpg"
        Genres.Clear()
        Mappings.Clear()
    End Sub

    ''' <summary>
    ''' Creates a deep copy of this clsXMLGenreMapping instance.
    ''' </summary>
    ''' <returns>A new clsXMLGenreMapping object with all properties copied.</returns>
    ''' <remarks>
    ''' Uses JSON serialization via Newtonsoft.Json to perform the deep clone.
    ''' This replaces the deprecated BinaryFormatter approach for security and 
    ''' future .NET compatibility.
    ''' </remarks>
    Public Function CloneDeep() As Object Implements ICloneable.Clone
        'Use JSON serialization for deep cloning (replaces deprecated BinaryFormatter)
        Dim json As String = JsonConvert.SerializeObject(Me)
        Return JsonConvert.DeserializeObject(Of clsXMLGenreMapping)(json)
    End Function

    Public Sub Load()
        If File.Exists(FileNameFullPath) Then
            Dim objStreamReader = New StreamReader(FileNameFullPath)
            Try
                Dim nXML = CType(New XmlSerializer([GetType]).Deserialize(objStreamReader), clsXMLGenreMapping)
                DefaultImage = nXML.DefaultImage
                Genres = nXML.Genres
                Mappings = nXML.Mappings
                objStreamReader.Close()

                ' Validate and clean up loaded genres
                ValidateLoadedGenres()
            Catch ex As Exception
                _Logger.Error(ex, New StackFrame().GetMethod().Name)
                objStreamReader.Close()
                FileUtils.Common.CreateFileBackup(FileNameFullPath)
                File.Delete(FileNameFullPath)
                Clear()
            End Try
        ElseIf SearchOlderVersions Then
            Load()
        Else
            Clear()
        End If
    End Sub

    ''' <summary>
    ''' Validates genres loaded from XML and logs warnings for invalid names
    ''' </summary>
    Private Sub ValidateLoadedGenres()
        For Each genre In Genres.ToList()
            If Not IsValidGenreName(genre.Name) Then
                _Logger.Warn(String.Format("Loaded genre '{0}' has invalid name (contains spaces or special characters). Consider renaming in Genre Mapping dialog.", genre.Name))
            End If
        Next
    End Sub

    Public Function RunMapping(ByRef listToBeMapped As List(Of String), Optional ByVal addNewInputs As Boolean = True) As Boolean
        Dim nResult As New List(Of String)
        Dim bNewInputAdded As Boolean
        If listToBeMapped.Count > 0 Then
            For Each aInput As String In listToBeMapped
                Dim existingInput As GenreMapping = Mappings.FirstOrDefault(Function(f) f.SearchString = aInput)
                If existingInput IsNot Nothing Then
                    nResult.AddRange(existingInput.MappedTo)
                ElseIf addNewInputs Then
                    If Not IsValidGenreName(aInput) Then
                        ' Try to auto-fix by converting spaces to hyphens
                        Dim suggestedName As String = SuggestValidGenreName(aInput)

                        If Not String.IsNullOrEmpty(suggestedName) Then
                            ' Successfully auto-fixed (had only spaces as invalid characters)
                            _Logger.Info(String.Format("Genre '{0}' auto-fixed to '{1}'", aInput, suggestedName))

                            ' Check if the fixed name already exists as a genre
                            Dim existingGenre As GenreProperty = Genres.FirstOrDefault(Function(f) f.Name = suggestedName)
                            If existingGenre Is Nothing Then
                                ' Add ONLY the fixed genre name
                                Genres.Add(New GenreProperty With {.Name = suggestedName})
                            End If

                            ' Create mapping from original to fixed name
                            Mappings.Add(New GenreMapping With {
                                         .MappedTo = New List(Of String) From {suggestedName},
                                         .SearchString = aInput
                                         })
                            nResult.Add(suggestedName)
                            bNewInputAdded = True
                        Else
                            ' Contains special characters - skip and create empty mapping
                            _Logger.Warn(String.Format("Genre '{0}' contains invalid characters and cannot be auto-fixed. Skipping.", aInput))

                            ' Create empty mapping to prevent re-processing
                            Mappings.Add(New GenreMapping With {
                                         .MappedTo = New List(Of String),
                                         .SearchString = aInput
                                         })
                            bNewInputAdded = True
                            ' Note: We don't add anything to nResult, so this genre will be filtered out
                        End If
                    Else
                        ' Genre name is valid - proceed normally
                        Dim gProperty As GenreProperty = Genres.FirstOrDefault(Function(f) f.Name = aInput)
                        If gProperty Is Nothing Then
                            Genres.Add(New GenreProperty With {.Name = aInput})
                        End If

                        Mappings.Add(New GenreMapping With {
                                     .MappedTo = New List(Of String) From {aInput},
                                     .SearchString = aInput
                                     })
                        nResult.Add(aInput)
                        bNewInputAdded = True
                    End If
                End If
            Next
        End If

        'Cleanup Mappings list (not important but nice)
        If bNewInputAdded Then
            Genres.Sort()
            Mappings.Sort()
            Save()
        End If

        'Cleanup for comparing
        nResult = nResult.Distinct().ToList()
        nResult.Sort()
        listToBeMapped.Sort()

        'Comparing (check if something has been changed)
        Dim bNoChanges = listToBeMapped.SequenceEqual(nResult)

        'Set nResult as mapping result
        listToBeMapped = nResult

        'Return if the list has been changed or not
        Return Not bNoChanges
    End Function

    Public Sub Save()
        Sort()
        Dim xmlSerial As New XmlSerializer(GetType(clsXMLGenreMapping))
        Dim xmlWriter As New StreamWriter(FileNameFullPath)
        xmlSerial.Serialize(xmlWriter, Me)
        xmlWriter.Close()
    End Sub

    Private Function SearchOlderVersions() As Boolean
#Disable Warning BC40000 'The type or member is obsolete.
        Dim strVersion1 = Path.Combine(Master.SettingsPath, "Core.Genres.xml")
        Select Case True
            Case File.Exists(strVersion1)
                Try
                    File.Move(strVersion1, FileNameFullPath)
                    Return True
                Catch ex As Exception
                    _Logger.Error(ex, New StackFrame().GetMethod().Name)
                    FileUtils.Common.CreateFileBackup(strVersion1)
                    File.Delete(strVersion1)
                    Return False
                End Try
            Case Else
                Return False
        End Select
#Enable Warning BC40000 'The type or member is obsolete.
    End Function

    Public Sub Sort()
        Genres.Sort()
        Mappings.Sort()
    End Sub

    ''' <summary>
    ''' Automatically matches genres to image files based on genre name
    ''' Converts genre name to lowercase and looks for matching image file
    ''' </summary>
    Public Sub AutoMatchImages(ByVal genresPath As String)
        If Not Directory.Exists(genresPath) Then Return

        Try
            ' Get all available image files
            Dim imageFiles As New List(Of String)
            imageFiles.AddRange(Directory.GetFiles(genresPath, "*.jpg"))
            imageFiles.AddRange(Directory.GetFiles(genresPath, "*.png"))

            ' Try to match genres without images
            For Each genre In Genres.Where(Function(g) String.IsNullOrEmpty(g.Image))
                ' Convert genre name to lowercase for matching
                Dim genreFileName As String = genre.Name.ToLower()

                ' Look for exact match (case-insensitive)
                Dim matchedFile = imageFiles.FirstOrDefault(Function(f) _
                    Path.GetFileNameWithoutExtension(f).ToLower() = genreFileName)

                If matchedFile IsNot Nothing Then
                    genre.Image = Path.GetFileName(matchedFile)
                    _Logger.Info(String.Format("Auto-matched genre '{0}' to image '{1}'", genre.Name, genre.Image))
                End If
            Next

        Catch ex As Exception
            _Logger.Error(ex, New StackFrame().GetMethod().Name)
        End Try
    End Sub

    ''' <summary>
    ''' Validates that a genre name follows the naming rules:
    ''' - Alphanumeric characters only
    ''' - Hyphens allowed (but not at start/end)
    ''' - No spaces or special characters
    ''' </summary>
    Public Shared Function IsValidGenreName(ByVal genreName As String) As Boolean
        If String.IsNullOrWhiteSpace(genreName) Then Return False

        ' Check for invalid characters (anything except letters, numbers, and hyphens)
        For Each c As Char In genreName
            If Not (Char.IsLetterOrDigit(c) OrElse c = "-"c) Then
                Return False
            End If
        Next

        ' Check that it doesn't start or end with a hyphen
        If genreName.StartsWith("-") OrElse genreName.EndsWith("-") Then
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Suggests a valid genre name by converting spaces to hyphens
    ''' Returns empty string if genre contains special characters (user must map manually)
    ''' </summary>
    Public Shared Function SuggestValidGenreName(ByVal genreName As String) As String
        If String.IsNullOrWhiteSpace(genreName) Then
            Return String.Empty
        End If

        Dim trimmedName As String = genreName.Trim()

        ' Check if it contains any special characters (except spaces and hyphens)
        For Each c As Char In trimmedName
            If Not (Char.IsLetterOrDigit(c) OrElse c = " "c OrElse c = "-"c) Then
                ' Contains special character - cannot auto-fix
                Return String.Empty
            End If
        Next

        ' Only contains letters, numbers, spaces, and hyphens - safe to auto-fix
        ' Replace spaces with hyphens
        Dim result As String = trimmedName.Replace(" ", "-")

        ' Remove any double hyphens
        While result.Contains("--")
            result = result.Replace("--", "-")
        End While

        ' Remove leading/trailing hyphens
        result = result.Trim("-"c)

        Return result
    End Function
#End Region 'Methods

End Class

<Serializable()>
Public Class GenreMapping
    Implements IComparable(Of GenreMapping)

#Region "Properties"

    <XmlElement("searchstring")>
    Public Property SearchString() As String = String.Empty

    <XmlElement("mappedto")>
    Public Property MappedTo() As List(Of String) = New List(Of String)

    <XmlElement("isnew")>
    Public Property isNew() As Boolean = True

#End Region 'Properties 

#Region "Methods"

    Public Function CompareTo(ByVal other As GenreMapping) As Integer _
        Implements IComparable(Of GenreMapping).CompareTo
        Try
            Dim retVal As Integer = (SearchString).CompareTo(other.SearchString)
            Return retVal
        Catch ex As Exception
            Return 0
        End Try
    End Function

#End Region 'Methods

End Class

<Serializable()>
Public Class GenreProperty
    Implements IComparable(Of GenreProperty)

#Region "Properties"

    <XmlElement("name")>
    Public Property Name() As String = String.Empty

    <XmlElement("image")>
    Public Property Image() As String = String.Empty

    <XmlElement("isnew")>
    Public Property isNew() As Boolean = True

#End Region 'Properties 

#Region "Methods"

    Public Function CompareTo(ByVal other As GenreProperty) As Integer _
        Implements IComparable(Of GenreProperty).CompareTo
        Try
            Dim retVal As Integer = (Name).CompareTo(other.Name)
            Return retVal
        Catch ex As Exception
            Return 0
        End Try
    End Function

#End Region 'Methods

End Class